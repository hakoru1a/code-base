using MediatR;
using Shared.DTOs.Product;

namespace Product.Application.Features.ProductVariants.Commands.UpdateVariant
{
    public class UpdateVariantCommand : IRequest<ProductVariantDto>
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public int InventoryQuantity { get; set; }
        public bool TrackInventory { get; set; } = true;
    }
}

