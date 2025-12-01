using Contracts.Domain.Interface;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates product name length does not exceed maximum allowed characters.
/// </summary>
public class ProductNameLengthRule : IBusinessRule
{
    private readonly string _name;
    private readonly int _maxLength;

    public ProductNameLengthRule(string name, int maxLength = 100)
    {
        _name = name;
        _maxLength = maxLength;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_name) && _name.Length > _maxLength;

    public string Message => $"Product name cannot exceed {_maxLength} characters.";

    public string Code => "Product.NameTooLong";
}
