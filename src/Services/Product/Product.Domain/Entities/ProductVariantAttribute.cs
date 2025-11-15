using Contracts.Domain;

namespace Product.Domain.Entities;

public class ProductVariantAttribute : EntityAuditBase<long>
{
    public long ProductVariantId { get; set; }
    public long AttributeDefId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public virtual ProductVariant ProductVariant { get; set; } = null!;
    public virtual AttributeDef AttributeDef { get; set; } = null!;
}