using System.Linq.Expressions;
using System.Reflection;

namespace TLBIOMASS.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string? direction)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query;

        var isDescending = direction?.ToLower() == "desc";
        
        // Handle underscore naming (ten_khach_hang -> TenKhachHang)
        var propertyName = ToPascalCase(sortBy);

        var entityType = typeof(T);
        var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return query;

        var parameter = Expression.Parameter(entityType, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { entityType, property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var words = input.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }

        return string.Join("", words);
    }
}
