using MediatR;
using Shared.DTOs.Product;

namespace Product.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<ProductDto>
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