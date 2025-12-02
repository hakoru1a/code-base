using Contracts.Domain;

namespace Generate.Domain.Products;

public class ProductDetail : AuditableBase<long>
{
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public virtual Product Product { get; private set; } = null!;

    // EF Core constructor
    private ProductDetail() { }

    // Domain constructor
    public ProductDetail(string description)
    {
        ValidateDescription(description);
        Description = description;
    }

    // Factory method
    public static ProductDetail Create(string description)
    {
        return new ProductDetail(description);
    }

    // Business methods
    public void UpdateDescription(string description)
    {
        ValidateDescription(description);
        Description = description;
    }

    public void AssignToProduct(Product product)
    {
        if (product == null)
            throw ProductError.CategoryCannotBeNull();

        Product = product;
    }

    // Domain validation
    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw ProductError.ProductDetailNotFound();

        if (description.Length > 1000) // TEXT field can be longer
            throw new Contracts.Exceptions.BusinessException("Product description cannot exceed 1000 characters");
    }

    // Business queries
    public bool HasDescription()
    {
        return !string.IsNullOrWhiteSpace(Description);
    }

    public string GetSummary(int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(Description))
            return "No description available";

        return Description.Length <= maxLength
            ? Description
            : Description.Substring(0, maxLength) + "...";
    }
}

