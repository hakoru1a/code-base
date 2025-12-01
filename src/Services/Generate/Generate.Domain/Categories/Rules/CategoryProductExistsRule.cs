using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a product exists in the category.
/// </summary>
public class CategoryProductExistsRule : IBusinessRule
{
    private readonly List<Product> _products;
    private readonly Product _product;

    public CategoryProductExistsRule(List<Product> products, Product product)
    {
        _products = products;
        _product = product;
    }

    public bool IsBroken() => !_products.Any(p => p.Id == _product.Id);

    public string Message => "Product not found in this category.";

    public string Code => "Category.ProductNotFound";
}
