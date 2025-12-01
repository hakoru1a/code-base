using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a product is required when adding to category.
/// </summary>
public class CategoryProductRequiredRule : IBusinessRule
{
    private readonly Product? _product;

    public CategoryProductRequiredRule(Product? product)
    {
        _product = product;
    }

    public bool IsBroken() => _product == null;

    public string Message => "Product cannot be null.";

    public string Code => "Category.ProductRequired";
}
