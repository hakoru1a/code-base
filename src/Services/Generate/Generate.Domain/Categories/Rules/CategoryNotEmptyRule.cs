using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a category is not empty (has at least one product).
/// </summary>
public class CategoryNotEmptyRule : IBusinessRule
{
    private readonly List<Product> _products;

    public CategoryNotEmptyRule(List<Product> products)
    {
        _products = products;
    }

    public bool IsBroken() => !_products.Any();

    public string Message => "Category must contain at least one product.";

    public string Code => "Category.CannotBeEmpty";
}
