using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check order có items không
/// </summary>
public class HasItemsSpecification : ISpecification<Order>
{
    public bool IsSatisfiedBy(Order order)
    {
        return order.OrderItems.Any();
    }
}

