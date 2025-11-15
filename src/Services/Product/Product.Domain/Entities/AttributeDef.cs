using Contracts.Domain;

namespace Product.Domain.Entities;

public class AttributeDef : EntityAuditBase<long>
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AttributeType Type { get; set; }
    public string? Options { get; set; } // JSON string for dropdown options
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; } = new List<ProductVariantAttribute>();
}

public enum AttributeType
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    Dropdown = 4,
    Color = 5,
    Size = 6
}