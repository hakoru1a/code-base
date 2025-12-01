using Contracts.Domain.Interface;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates product name format (no leading or trailing spaces).
/// </summary>
public class ProductNameFormatRule : IBusinessRule
{
    private readonly string _name;

    public ProductNameFormatRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_name) && _name.Trim() != _name;

    public string Message => "Product name cannot have leading or trailing spaces.";

    public string Code => "Product.NameInvalidFormat";
}
