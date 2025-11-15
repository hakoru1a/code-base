using Contracts.Domain;

namespace Product.Domain.Entities;

public class Product : EntityAuditBase<long>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public long? CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

public enum ProductStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}