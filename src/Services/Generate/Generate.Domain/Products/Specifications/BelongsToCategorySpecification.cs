using Contracts.Domain.Interface;
using Generate.Domain.Categories;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product thuộc về category cụ thể
/// </summary>
public class BelongsToCategorySpecification : ISpecification<Product>
{
    private readonly Category _category;

    public BelongsToCategorySpecification(Category category)
    {
        _category = category ?? throw ProductError.CategoryCannotBeNull();
    }

    public bool IsSatisfiedBy(Product product)
    {
        return product.Category != null && product.Category.Id == _category.Id;
    }
}

