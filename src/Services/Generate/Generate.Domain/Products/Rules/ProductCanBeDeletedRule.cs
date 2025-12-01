using Contracts.Domain.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates a product can be deleted (no existing orders).
/// </summary>
public class ProductCanBeDeletedRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;

    public ProductCanBeDeletedRule(List<OrderItem> orderItems)
    {
        _orderItems = orderItems;
    }

    public bool IsBroken() => _orderItems.Any();

    public string Message => "Cannot delete product that has existing orders.";

    public string Code => "Product.CannotDeleteWithOrders";
}
