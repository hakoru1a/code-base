using System.Linq.Expressions;

namespace Shared.Extensions;

/// <summary>
/// Extension methods để apply dynamic sorting cho IQueryable
/// </summary>
public static class QueryableSortExtensions
{
    /// <summary>
    /// Apply sorting động dựa trên tên property và direction
    /// </summary>
    /// <typeparam name="TEntity">Kiểu entity cần sort</typeparam>
    /// <param name="query">IQueryable cần sort</param>
    /// <param name="orderBy">Tên property cần sort (case-insensitive)</param>
    /// <param name="direction">Hướng sort: "asc" hoặc "desc" (mặc định: "asc")</param>
    /// <returns>IQueryable đã được sort</returns>
    /// <example>
    /// <code>
    /// var query = dbContext.Categories.AsQueryable();
    /// var sorted = query.ApplySort("Name", "desc");
    /// </code>
    /// </example>
    public static IQueryable<TEntity> ApplySort<TEntity>(
        this IQueryable<TEntity> query,
        string? orderBy,
        string? direction = "asc")
    {
        // Nếu không có orderBy, return query gốc
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return query;
        }

        // Normalize direction
        var isDescending = direction?.ToLower() == "desc";

        // Tìm property trên entity (case-insensitive)
        var entityType = typeof(TEntity);
        var property = entityType.GetProperties()
            .FirstOrDefault(p => p.Name.Equals(orderBy, StringComparison.OrdinalIgnoreCase));

        // Nếu không tìm thấy property, return query gốc (không throw exception)
        if (property == null)
        {
            return query;
        }

        // Build expression tree: x => x.PropertyName
        var parameter = Expression.Parameter(entityType, "x");
        var propertyAccess = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);

        // Gọi OrderBy hoặc OrderByDescending
        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { entityType, property.PropertyType },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<TEntity>(resultExpression);
    }

    /// <summary>
    /// Apply sorting với nhiều columns (ThenBy support)
    /// </summary>
    /// <typeparam name="TEntity">Kiểu entity cần sort</typeparam>
    /// <param name="query">IQueryable cần sort</param>
    /// <param name="sortExpressions">Danh sách các sort expression (property name + direction)</param>
    /// <returns>IQueryable đã được sort</returns>
    /// <example>
    /// <code>
    /// var sorted = query.ApplyMultiSort(new[] 
    /// { 
    ///     ("Name", "asc"), 
    ///     ("CreatedDate", "desc") 
    /// });
    /// </code>
    /// </example>
    public static IQueryable<TEntity> ApplyMultiSort<TEntity>(
        this IQueryable<TEntity> query,
        params (string PropertyName, string Direction)[] sortExpressions)
    {
        if (sortExpressions == null || sortExpressions.Length == 0)
        {
            return query;
        }

        IOrderedQueryable<TEntity>? orderedQuery = null;

        for (int i = 0; i < sortExpressions.Length; i++)
        {
            var (propertyName, direction) = sortExpressions[i];
            var isDescending = direction?.ToLower() == "desc";

            var entityType = typeof(TEntity);
            var property = entityType.GetProperties()
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null) continue;

            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            string methodName;
            if (i == 0)
            {
                methodName = isDescending ? "OrderByDescending" : "OrderBy";
            }
            else
            {
                methodName = isDescending ? "ThenByDescending" : "ThenBy";
            }

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { entityType, property.PropertyType },
                i == 0 ? query.Expression : orderedQuery!.Expression,
                Expression.Quote(lambda));

            orderedQuery = (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(resultExpression);
        }

        return orderedQuery ?? query;
    }
}
