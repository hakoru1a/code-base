namespace Shared.DTOs.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }

        public List<CreateProductVariantRequest> Variants { get; set; } = new List<CreateProductVariantRequest>();
    }

    public class CreateProductVariantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public int InventoryQuantity { get; set; }
        public bool TrackInventory { get; set; } = true;

        public List<CreateProductVariantAttributeRequest> Attributes { get; set; } = new List<CreateProductVariantAttributeRequest>();
    }

    public class CreateProductVariantAttributeRequest
    {
        public long AttributeDefId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class UpdateProductRequest
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
    }

}