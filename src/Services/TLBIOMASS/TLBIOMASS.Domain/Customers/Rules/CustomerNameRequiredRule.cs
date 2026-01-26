using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Customers.Rules;

public class CustomerNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public CustomerNameRequiredRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Customer name is required";
    public string Code => "Customer.NameRequired";
}
