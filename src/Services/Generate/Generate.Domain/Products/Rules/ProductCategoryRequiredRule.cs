using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates a category is required when assigning to product.
/// </summary>
public class ProductCategoryRequiredRule : IBusinessRule
{
    private readonly Category? _category;

    public ProductCategoryRequiredRule(Category? category)
    {
        _category = category;
    }

    public bool IsBroken() => _category == null;

    public string Message => "Category cannot be null when assigning to product.";

    public string Code => "Product.CategoryRequired";
}
