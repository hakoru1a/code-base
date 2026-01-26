using Contracts.Domain.Interface;
using System.Text.RegularExpressions;

namespace TLBIOMASS.Domain.Customers.Rules;

public class CustomerEmailFormatRule : IBusinessRule
{
    private readonly string? _email;

    public CustomerEmailFormatRule(string? email)
    {
        _email = email;
    }

    public bool IsBroken()
    {
        if (string.IsNullOrEmpty(_email)) return false;
        
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return !regex.IsMatch(_email);
    }

    public string Message => "Invalid email format";
    public string Code => "Customer.EmailInvalid";
}
