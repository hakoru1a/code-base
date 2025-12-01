using Contracts.Domain.Interface;
using Generate.Domain.Categories;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có chứa product cụ thể không
/// </summary>
public class ContainsProductSpecification : ISpecification<Category>
{
    private readonly Product _product;

    public ContainsProductSpecification(Product product)
    {
        _product = product ?? throw CategoryError.ProductCannotBeNull();
    }

    public bool IsSatisfiedBy(Category category)
    {
        return category.Products.Any(p => p.Id == _product.Id);
    }
}

