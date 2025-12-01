using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check order có thể bị xóa không
/// </summary>
public class CanBeDeletedSpecification : ISpecification<Order>
{
    public bool IsSatisfiedBy(Order order)
    {
        return !order.OrderItems.Any();
    }
}

