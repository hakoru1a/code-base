using Contracts.Domain.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories.Specifications;

/// <summary>
/// Specification để check category có popular không (nhiều orders)
/// </summary>
public class IsPopularCategorySpecification : ISpecification<Category>
{
    private readonly int _orderThreshold;

    public IsPopularCategorySpecification(int orderThreshold = 100)
    {
        if (orderThreshold <= 0)
            throw new ArgumentException("Order threshold must be greater than zero");

        _orderThreshold = orderThreshold;
    }

    public bool IsSatisfiedBy(Category category)
    {
        var totalOrders = category.Products.SelectMany(p => p.OrderItems).Count();
        return totalOrders >= _orderThreshold;
    }
}

