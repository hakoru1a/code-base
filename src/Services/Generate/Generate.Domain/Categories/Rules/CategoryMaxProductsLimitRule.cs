using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a category does not exceed maximum products limit.
/// </summary>
public class CategoryMaxProductsLimitRule : IBusinessRule
{
    private readonly List<Product> _products;
    private readonly int _maxProducts;

    public CategoryMaxProductsLimitRule(List<Product> products, int maxProducts = 1000)
    {
        _products = products;
        _maxProducts = maxProducts;
    }

    public bool IsBroken() => _products.Count >= _maxProducts;

    public string Message => $"Category cannot contain more than {_maxProducts} products.";

    public string Code => "Category.MaxProductsExceeded";
}
