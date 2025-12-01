using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có active products không
/// </summary>
public class HasActiveProductsSpecification : ISpecification<Category>
{
    public bool IsSatisfiedBy(Category category)
    {
        return category.Products.Any(p => p.OrderItems.Any());
    }
}

