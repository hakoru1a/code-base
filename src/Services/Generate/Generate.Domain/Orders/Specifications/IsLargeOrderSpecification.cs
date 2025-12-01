using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check order có phải là large order không
/// </summary>
public class IsLargeOrderSpecification : ISpecification<Order>
{
    private readonly int _threshold;

    public IsLargeOrderSpecification(int threshold = 50)
    {
        if (threshold <= 0)
            throw OrderError.InvalidThreshold(threshold);
        
        _threshold = threshold;
    }

    public bool IsSatisfiedBy(Order order)
    {
        var totalItems = order.OrderItems.Sum(oi => oi.Quantity);
        return totalItems >= _threshold;
    }
}

