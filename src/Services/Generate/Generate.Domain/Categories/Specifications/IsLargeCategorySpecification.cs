using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có phải là large category không
/// </summary>
public class IsLargeCategorySpecification : ISpecification<Category>
{
    private readonly int _threshold;

    public IsLargeCategorySpecification(int threshold = 50)
    {
        if (threshold <= 0)
            throw new ArgumentException("Threshold must be greater than zero");

        _threshold = threshold;
    }

    public bool IsSatisfiedBy(Category category)
    {
        return category.Products.Count >= _threshold;
    }
}

