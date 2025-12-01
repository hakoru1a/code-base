using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Products.Specifications;

/// <summary>
/// Specification để check product có orders trong date range
/// </summary>
public class HasOrdersInDateRangeSpecification : ISpecification<Product>
{
    private readonly DateTime _fromDate;
    private readonly DateTime _toDate;

    public HasOrdersInDateRangeSpecification(DateTime fromDate, DateTime toDate)
    {
        if (fromDate > toDate)
            throw new ArgumentException("From date must be less than or equal to to date");

        _fromDate = fromDate;
        _toDate = toDate;
    }

    public bool IsSatisfiedBy(Product product)
    {
        return product.OrderItems.Any(oi =>
            oi.CreatedDate >= _fromDate && oi.CreatedDate <= _toDate);
    }
}

