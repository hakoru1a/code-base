using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates an order item is required (not null).
/// </summary>
public class ProductOrderItemRequiredRule : IBusinessRule
{
    private readonly OrderItem? _orderItem;

    public ProductOrderItemRequiredRule(OrderItem? orderItem)
    {
        _orderItem = orderItem;
    }

    public bool IsBroken() => _orderItem == null;

    public string Message => "Order item cannot be null.";

    public string Code => "Product.OrderItemRequired";
}
