using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có high volume không (quantity lớn)
/// </summary>
public class IsHighVolumeProductSpecification : ISpecification<Product>
{
    private readonly decimal _volumeThreshold;

    public IsHighVolumeProductSpecification(decimal volumeThreshold = 100)
    {
        if (volumeThreshold <= 0)
            throw new ArgumentException("Volume threshold must be greater than zero");
        
        _volumeThreshold = volumeThreshold;
    }

    public bool IsSatisfiedBy(Product product)
    {
        var totalVolume = product.OrderItems.Sum(oi => oi.Quantity);
        return totalVolume >= _volumeThreshold;
    }
}

