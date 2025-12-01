using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có product detail không
/// </summary>
public class HasProductDetailSpecification : ISpecification<Product>
{
    public bool IsSatisfiedBy(Product product)
    {
        return product.ProductDetail != null;
    }
}

