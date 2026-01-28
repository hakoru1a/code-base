using System.Linq.Expressions;
using System.Reflection;
using Shared.DTOs;

namespace Shared.Extensions;

/// <summary>
/// Extension methods để apply filters tự động từ FilterDTO lên IQueryable
/// </summary>
public static class QueryableFilterExtensions
{
    /// <summary>
    /// Tự động apply tất cả filters từ DTO lên query
    /// </summary>
    /// <typeparam name="TEntity">Kiểu entity trong database</typeparam>
    /// <typeparam name="TFilter">Kiểu filter DTO</typeparam>
    /// <param name="query">IQueryable cần filter</param>
    /// <param name="filter">Filter DTO chứa các điều kiện filter</param>
    /// <returns>IQueryable đã được filter</returns>
    /// <example>
    /// <code>
    /// var query = dbContext.Categories.AsQueryable();
    /// var filtered = query.ApplyFilters(categoryFilter);
    /// </code>
    /// </example>
    public static IQueryable<TEntity> ApplyFilters<TEntity, TFilter>(
        this IQueryable<TEntity> query,
        TFilter filter)
        where TFilter : BaseFilterDto
    {
        if (filter == null)
        {
            return query;
        }

        var filterType = typeof(TFilter);
        var entityType = typeof(TEntity);

        // Lấy tất cả properties từ filter DTO
        var filterProperties = filterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var filterProp in filterProperties)
        {
            // Bỏ qua các properties từ BaseFilterDto (PageNumber, PageSize, OrderBy, etc.)
            if (IsBaseFilterProperty(filterProp.Name))
            {
                continue;
            }

            // Lấy giá trị của filter property
            var filterValue = filterProp.GetValue(filter);

            // Nếu null hoặc empty string, bỏ qua
            if (filterValue == null || (filterValue is string str && string.IsNullOrWhiteSpace(str)))
            {
                continue;
            }

            // Tìm property tương ứng trên entity
            var entityProp = FindEntityProperty(entityType, filterProp.Name);
            if (entityProp == null)
            {
                continue;
            }

            // Apply filter dựa trên kiểu dữ liệu
            query = ApplyFilterForProperty(query, entityProp, filterProp, filterValue);
        }

        return query;
    }

    /// <summary>
    /// Kiểm tra xem property có phải từ BaseFilterDto không
    /// </summary>
    private static bool IsBaseFilterProperty(string propertyName)
    {
        return propertyName is "PageNumber" or "PageSize" or "OrderBy" or "OrderByDirection" or "SearchTerms";
    }

    /// <summary>
    /// Tìm property trên entity tương ứng với filter property
    /// Hỗ trợ các pattern: Name -> Name, CreatedFrom -> CreatedDate, CreatedTo -> CreatedDate
    /// </summary>
    private static PropertyInfo? FindEntityProperty(Type entityType, string filterPropertyName)
    {
        // Thử tìm exact match trước
        var exactMatch = entityType.GetProperty(filterPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Xử lý các pattern đặc biệt: CreatedFrom/CreatedTo -> CreatedDate
        if (filterPropertyName.EndsWith("From") || filterPropertyName.EndsWith("To"))
        {
            var baseName = filterPropertyName.Replace("From", "").Replace("To", "");
            var baseProperty = entityType.GetProperty(baseName, BindingFlags.Public | BindingFlags.Instance);
            if (baseProperty != null)
            {
                return baseProperty;
            }

            // Thử thêm "Date" vào cuối: Created -> CreatedDate
            var dateProperty = entityType.GetProperty(baseName + "Date", BindingFlags.Public | BindingFlags.Instance);
            if (dateProperty != null)
            {
                return dateProperty;
            }
        }

        // Case-insensitive search
        return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals(filterPropertyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Apply filter cho một property cụ thể
    /// </summary>
    private static IQueryable<TEntity> ApplyFilterForProperty<TEntity>(
        IQueryable<TEntity> query,
        PropertyInfo entityProp,
        PropertyInfo filterProp,
        object filterValue)
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");
        var property = Expression.Property(parameter, entityProp);

        Expression? filterExpression = null;

        // Xử lý theo kiểu dữ liệu
        if (filterProp.PropertyType == typeof(string))
        {
            // String: sử dụng Contains
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var constant = Expression.Constant(filterValue, typeof(string));
            filterExpression = Expression.Call(property, containsMethod, constant);
        }
        else if (IsDateTimeType(filterProp.PropertyType))
        {
            // DateTime/DateTimeOffset: xử lý range (From/To)
            if (filterProp.Name.EndsWith("From"))
            {
                // GreaterThanOrEqual
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.GreaterThanOrEqual(property, constant);
            }
            else if (filterProp.Name.EndsWith("To"))
            {
                // LessThanOrEqual
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.LessThanOrEqual(property, constant);
            }
            else
            {
                // Exact match
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.Equal(property, constant);
            }
        }
        else if (IsNumericType(filterProp.PropertyType))
        {
            // Numeric: xử lý range hoặc exact match
            if (filterProp.Name.EndsWith("From") || filterProp.Name.Contains("Min"))
            {
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.GreaterThanOrEqual(property, constant);
            }
            else if (filterProp.Name.EndsWith("To") || filterProp.Name.Contains("Max"))
            {
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.LessThanOrEqual(property, constant);
            }
            else
            {
                var constant = Expression.Constant(filterValue, entityProp.PropertyType);
                filterExpression = Expression.Equal(property, constant);
            }
        }
        else if (filterProp.PropertyType == typeof(bool) || filterProp.PropertyType == typeof(bool?))
        {
            // Boolean: exact match
            var constant = Expression.Constant(filterValue, entityProp.PropertyType);
            filterExpression = Expression.Equal(property, constant);
        }
        else
        {
            // Default: exact match
            var constant = Expression.Constant(filterValue, entityProp.PropertyType);
            filterExpression = Expression.Equal(property, constant);
        }

        if (filterExpression != null)
        {
            var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Kiểm tra xem type có phải DateTime không
    /// </summary>
    private static bool IsDateTimeType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(DateTime) || 
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(DateOnly);
    }

    /// <summary>
    /// Kiểm tra xem type có phải numeric không
    /// </summary>
    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(int) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(byte);
    }
}
