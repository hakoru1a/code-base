using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có trong category không
/// </summary>
public class IsInCategorySpecification : ISpecification<Product>
{
    public bool IsSatisfiedBy(Product product)
    {
        return product.Category != null;
    }
}

