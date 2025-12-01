using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders.Specifications;

/// <summary>
/// Specification để check order value trong khoảng
/// </summary>
public class OrderValueRangeSpecification : ISpecification<Order>
{
    private readonly decimal _minValue;
    private readonly decimal _maxValue;

    public OrderValueRangeSpecification(decimal minValue, decimal maxValue)
    {
        if (minValue < 0) throw new ArgumentException("Min value cannot be negative");
        if (maxValue < minValue) throw new ArgumentException("Max value must be greater than min value");
        
        _minValue = minValue;
        _maxValue = maxValue;
    }

    public bool IsSatisfiedBy(Order order)
    {
        // Tạm thời dùng total quantity, sau này có thể thay bằng pricing logic
        var totalValue = order.OrderItems.Sum(oi => oi.Quantity);
        return totalValue >= _minValue && totalValue <= _maxValue;
    }
}

