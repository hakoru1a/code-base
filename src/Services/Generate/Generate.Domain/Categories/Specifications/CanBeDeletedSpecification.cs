using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có thể bị xóa không
/// </summary>
public class CanBeDeletedSpecification : ISpecification<Category>
{
    public bool IsSatisfiedBy(Category category)
    {
        return !category.Products.Any();
    }
}

