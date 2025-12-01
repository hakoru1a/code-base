using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category name theo pattern
/// </summary>
public class CategoryNamePatternSpecification : ISpecification<Category>
{
    private readonly string _pattern;

    public CategoryNamePatternSpecification(string pattern)
    {
        _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() :
            throw new ArgumentException("Pattern cannot be null or empty");
    }

    public bool IsSatisfiedBy(Category category)
    {
        return category.Name.ToLowerInvariant().Contains(_pattern);
    }
}

