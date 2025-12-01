using Shared.DTOs.Product;
using MediatR;

namespace Generate.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductResponseDto>
    {
        public long Id { get; set; }
    }
}

