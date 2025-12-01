using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có thể bị xóa không
/// </summary>
public class CanBeDeletedSpecification : ISpecification<Product>
{
    public bool IsSatisfiedBy(Product product)
    {
        return !product.OrderItems.Any();
    }
}

