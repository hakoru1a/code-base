using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates a product does not already exist in the order.
/// </summary>
public class OrderProductNotExistsRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;
    private readonly Product _product;

    public OrderProductNotExistsRule(List<OrderItem> orderItems, Product product)
    {
        _orderItems = orderItems;
        _product = product;
    }

    public bool IsBroken() => _orderItems.Any(oi => ReferenceEquals(oi.Product, _product));

    public string Message => "Product already exists in this order.";

    public string Code => "Order.ProductAlreadyExists";
}
