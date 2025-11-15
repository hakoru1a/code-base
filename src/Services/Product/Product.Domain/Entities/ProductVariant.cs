using Contracts.Domain;

namespace Product.Domain.Entities;

public class ProductVariant : EntityAuditBase<long>
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public int InventoryQuantity { get; set; }
    public bool TrackInventory { get; set; } = true;
    public ProductVariantStatus Status { get; set; } = ProductVariantStatus.Active;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<ProductVariantAttribute> Attributes { get; set; } = new List<ProductVariantAttribute>();
}

public enum ProductVariantStatus
{
    Active = 1,
    Inactive = 0
}