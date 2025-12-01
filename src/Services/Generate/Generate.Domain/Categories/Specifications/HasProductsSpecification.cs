using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có products không
/// </summary>
public class HasProductsSpecification : ISpecification<Category>
{
    public bool IsSatisfiedBy(Category category)
    {
        return category.Products.Any();
    }
}

