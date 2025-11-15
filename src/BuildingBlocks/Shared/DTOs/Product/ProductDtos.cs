namespace Shared.DTOs.Product
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public int Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }

    public class ProductVariantDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public int InventoryQuantity { get; set; }
        public bool TrackInventory { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        
        public List<ProductVariantAttributeDto> Attributes { get; set; } = new List<ProductVariantAttributeDto>();
    }

    public class ProductVariantAttributeDto
    {
        public long Id { get; set; }
        public long ProductVariantId { get; set; }
        public long AttributeDefId { get; set; }
        public string Value { get; set; } = string.Empty;
        
        public AttributeDefDto? AttributeDef { get; set; }
    }

    public class AttributeDefDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}