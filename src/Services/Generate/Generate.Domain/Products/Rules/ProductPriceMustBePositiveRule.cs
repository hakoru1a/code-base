using Contracts.Domain.Interface;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates product price must be greater than 0.
/// </summary>
public class ProductPriceMustBePositiveRule : IBusinessRule
{
    private readonly decimal _price;

    public ProductPriceMustBePositiveRule(decimal price)
    {
        _price = price;
    }

    public bool IsBroken() => _price <= 0;

    public string Message => "Product price must be greater than 0.";

    public string Code => "Product.PriceMustBePositive";
}
