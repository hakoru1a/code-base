using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a product does not already exist in the category.
/// </summary>
public class CategoryProductNotExistsRule : IBusinessRule
{
    private readonly List<Product> _products;
    private readonly Product _product;

    public CategoryProductNotExistsRule(List<Product> products, Product product)
    {
        _products = products;
        _product = product;
    }

    public bool IsBroken() => _products.Any(p => p.Id == _product.Id);

    public string Message => "Product already exists in this category.";

    public string Code => "Category.ProductAlreadyExists";
}
