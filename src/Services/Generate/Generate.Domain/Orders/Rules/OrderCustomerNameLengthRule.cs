using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates customer name length does not exceed maximum allowed characters.
/// </summary>
public class OrderCustomerNameLengthRule : IBusinessRule
{
    private readonly string _customerName;
    private readonly int _maxLength;

    public OrderCustomerNameLengthRule(string customerName, int maxLength = 100)
    {
        _customerName = customerName;
        _maxLength = maxLength;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_customerName) && _customerName.Length > _maxLength;

    public string Message => $"Customer name cannot exceed {_maxLength} characters.";

    public string Code => "Order.CustomerNameTooLong";
}
