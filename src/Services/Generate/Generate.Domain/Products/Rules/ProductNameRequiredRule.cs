using Contracts.Domain.Interface;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates a product name is required (not null, empty, or whitespace).
/// </summary>
public class ProductNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public ProductNameRequiredRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Product name is required.";

    public string Code => "Product.NameRequired";
}
