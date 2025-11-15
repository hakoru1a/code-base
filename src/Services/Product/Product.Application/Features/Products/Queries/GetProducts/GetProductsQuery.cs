using MediatR;
using Shared.DTOs.Product;
using Shared.SeedWork;

namespace Product.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public long? CategoryId { get; set; }
        public int? Status { get; set; }
    }
}