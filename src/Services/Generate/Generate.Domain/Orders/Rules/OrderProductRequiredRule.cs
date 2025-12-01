using Contracts.Domain.Interface;
using Generate.Domain.Products;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates a product is required when adding to order.
/// </summary>
public class OrderProductRequiredRule : IBusinessRule
{
    private readonly Product? _product;

    public OrderProductRequiredRule(Product? product)
    {
        _product = product;
    }

    public bool IsBroken() => _product == null;

    public string Message => "Product cannot be null.";

    public string Code => "Order.ProductRequired";
}
