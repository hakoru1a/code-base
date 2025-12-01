using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates a category can be deleted (no products).
/// </summary>
public class CategoryCanBeDeletedRule : IBusinessRule
{
    private readonly List<Product> _products;

    public CategoryCanBeDeletedRule(List<Product> products)
    {
        _products = products;
    }

    public bool IsBroken() => _products.Any();

    public string Message => "Cannot delete category that contains products.";

    public string Code => "Category.CannotDeleteWithProducts";
}
