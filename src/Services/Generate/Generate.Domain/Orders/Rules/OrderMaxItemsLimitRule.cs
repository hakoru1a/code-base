using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates an order does not exceed maximum items limit.
/// </summary>
public class OrderMaxItemsLimitRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;
    private readonly int _maxItems;

    public OrderMaxItemsLimitRule(List<OrderItem> orderItems, int maxItems = 100)
    {
        _orderItems = orderItems;
        _maxItems = maxItems;
    }

    public bool IsBroken() => _orderItems.Count >= _maxItems;

    public string Message => $"Order cannot contain more than {_maxItems} items.";

    public string Code => "Order.MaxItemsExceeded";
}
