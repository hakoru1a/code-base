using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates an order item exists for the product.
/// </summary>
public class ProductOrderItemExistsRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;
    private readonly OrderItem _orderItem;

    public ProductOrderItemExistsRule(List<OrderItem> orderItems, OrderItem orderItem)
    {
        _orderItems = orderItems;
        _orderItem = orderItem;
    }

    public bool IsBroken()
    {
        return !_orderItems.Any(oi =>
            ReferenceEquals(oi.Order, _orderItem.Order) &&
            ReferenceEquals(oi.Product, _orderItem.Product));
    }

    public string Message => "Order item not found for this product.";

    public string Code => "Product.OrderItemNotFound";
}
