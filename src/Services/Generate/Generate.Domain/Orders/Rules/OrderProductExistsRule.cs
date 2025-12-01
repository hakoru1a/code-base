using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates a product exists in the order.
/// </summary>
public class OrderProductExistsRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;
    private readonly Product _product;

    public OrderProductExistsRule(List<OrderItem> orderItems, Product product)
    {
        _orderItems = orderItems;
        _product = product;
    }

    public bool IsBroken() => !_orderItems.Any(oi => ReferenceEquals(oi.Product, _product));

    public string Message => "Product not found in this order.";

    public string Code => "Order.ProductNotFound";
}
