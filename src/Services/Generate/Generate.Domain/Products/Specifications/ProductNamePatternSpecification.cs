using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product name theo pattern
/// </summary>
public class ProductNamePatternSpecification : ISpecification<Product>
{
    private readonly string _pattern;

    public ProductNamePatternSpecification(string pattern)
    {
        _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() : 
            throw new ArgumentException("Pattern cannot be null or empty");
    }

    public bool IsSatisfiedBy(Product product)
    {
        return product.Name.ToLowerInvariant().Contains(_pattern);
    }
}

