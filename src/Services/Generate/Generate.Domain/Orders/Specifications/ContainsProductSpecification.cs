using Contracts.Domain.Interface;
using Generate.Domain.Orders;
using Generate.Domain.Products;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check order có chứa product cụ thể không
/// </summary>
public class ContainsProductSpecification : ISpecification<Order>
{
    private readonly Product _product;

    public ContainsProductSpecification(Product product)
    {
        _product = product ?? throw OrderError.ProductCannotBeNull();
    }

    public bool IsSatisfiedBy(Order order)
    {
        return order.OrderItems.Any(oi => ReferenceEquals(oi.Product, _product));
    }
}

