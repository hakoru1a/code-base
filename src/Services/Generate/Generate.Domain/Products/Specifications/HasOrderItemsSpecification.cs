using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có order items không
/// </summary>
public class HasOrderItemsSpecification : ISpecification<Product>
{
    public bool IsSatisfiedBy(Product product)
    {
        return product.OrderItems.Any();
    }
}

