using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates an order is not empty (has at least one item).
/// </summary>
public class OrderNotEmptyRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;

    public OrderNotEmptyRule(List<OrderItem> orderItems)
    {
        _orderItems = orderItems;
    }

    public bool IsBroken() => !_orderItems.Any();

    public string Message => "Order must contain at least one item.";

    public string Code => "Order.CannotBeEmpty";
}
