using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates an order can be deleted (no items).
/// </summary>
public class OrderCanBeDeletedRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;

    public OrderCanBeDeletedRule(List<OrderItem> orderItems)
    {
        _orderItems = orderItems;
    }

    public bool IsBroken() => _orderItems.Any();

    public string Message => "Cannot delete order that contains items.";

    public string Code => "Order.CannotDeleteWithItems";
}
