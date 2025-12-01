using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates customer name is required (not null, empty, or whitespace).
/// </summary>
public class OrderCustomerNameRequiredRule : IBusinessRule
{
    private readonly string _customerName;

    public OrderCustomerNameRequiredRule(string customerName)
    {
        _customerName = customerName;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_customerName);

    public string Message => "Customer name cannot be empty.";

    public string Code => "Order.CustomerNameRequired";
}
