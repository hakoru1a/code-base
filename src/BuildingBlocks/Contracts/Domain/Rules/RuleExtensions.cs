using Contracts.Domain.Interface;
using Contracts.Domain.Rules;

namespace Contracts.Domain.Rules;

/// <summary>
/// Extension methods để dễ sử dụng business rules
/// </summary>
public static class RuleExtensions
{
    /// <summary>
    /// Kết hợp hai rules bằng AND (cả hai phải pass)
    /// </summary>
    public static CompositeRule.AndRule And(
        this IBusinessRule left,
        IBusinessRule right)
    {
        return new CompositeRule.AndRule(left, right);
    }

    /// <summary>
    /// Kết hợp hai rules bằng OR (ít nhất một phải pass)
    /// </summary>
    public static CompositeRule.OrRule Or(
        this IBusinessRule left,
        IBusinessRule right)
    {
        return new CompositeRule.OrRule(left, right);
    }
}

