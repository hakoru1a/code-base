using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check customer name theo pattern
/// </summary>
public class CustomerNamePatternSpecification : ISpecification<Order>
{
    private readonly string _pattern;

    public CustomerNamePatternSpecification(string pattern)
    {
        _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() : 
            throw new ArgumentException("Pattern cannot be null or empty");
    }

    public bool IsSatisfiedBy(Order order)
    {
        return order.CustomerName.ToLowerInvariant().Contains(_pattern);
    }
}

