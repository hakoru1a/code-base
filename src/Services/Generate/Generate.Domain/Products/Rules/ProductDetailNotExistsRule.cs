using Contracts.Domain.Interface;

namespace Generate.Domain.Products.Rules;

/// <summary>
/// Business rule that validates a product detail does not already exist.
/// </summary>
public class ProductDetailNotExistsRule : IBusinessRule
{
    private readonly ProductDetail? _existingDetail;

    public ProductDetailNotExistsRule(ProductDetail? existingDetail)
    {
        _existingDetail = existingDetail;
    }

    public bool IsBroken() => _existingDetail != null;

    public string Message => "Product detail already exists for this product.";

    public string Code => "Product.DetailAlreadyExists";
}
