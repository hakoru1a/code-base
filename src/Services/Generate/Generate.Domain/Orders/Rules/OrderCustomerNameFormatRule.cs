using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates customer name format (no leading or trailing spaces).
/// </summary>
public class OrderCustomerNameFormatRule : IBusinessRule
{
    private readonly string _customerName;

    public OrderCustomerNameFormatRule(string customerName)
    {
        _customerName = customerName;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_customerName) && _customerName.Trim() != _customerName;

    public string Message => "Customer name cannot have leading or trailing spaces.";

    public string Code => "Order.CustomerNameInvalidFormat";
}
