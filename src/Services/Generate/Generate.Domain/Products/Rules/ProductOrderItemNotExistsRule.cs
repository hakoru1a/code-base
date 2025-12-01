using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates an order item does not already exist for the product.
/// </summary>
public class ProductOrderItemNotExistsRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;
    private readonly OrderItem _newOrderItem;

    public ProductOrderItemNotExistsRule(List<OrderItem> orderItems, OrderItem newOrderItem)
    {
        _orderItems = orderItems;
        _newOrderItem = newOrderItem;
    }

    public bool IsBroken()
    {
        return _orderItems.Any(oi =>
            ReferenceEquals(oi.Order, _newOrderItem.Order) &&
            ReferenceEquals(oi.Product, _newOrderItem.Product));
    }

    public string Message => "Order item already exists for this product.";

    public string Code => "Product.OrderItemAlreadyExists";
}
