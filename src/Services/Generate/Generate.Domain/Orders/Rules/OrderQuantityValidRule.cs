using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates order item quantity must be greater than zero.
/// </summary>
public class OrderQuantityValidRule : IBusinessRule
{
    private readonly int _quantity;

    public OrderQuantityValidRule(int quantity)
    {
        _quantity = quantity;
    }

    public bool IsBroken() => _quantity <= 0;

    public string Message => "Quantity must be greater than zero.";

    public string Code => "Order.InvalidQuantity";
}
