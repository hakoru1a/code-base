using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có phải là popular product không
/// </summary>
public class IsPopularProductSpecification : ISpecification<Product>
{
    private readonly int _orderThreshold;

    public IsPopularProductSpecification(int orderThreshold = 10)
    {
        if (orderThreshold <= 0)
            throw new ArgumentException("Order threshold must be greater than zero");
        
        _orderThreshold = orderThreshold;
    }

    public bool IsSatisfiedBy(Product product)
    {
        return product.OrderItems.Count >= _orderThreshold;
    }
}

