using Generate.Application.Common.DTOs.Product;
using MediatR;

namespace Generate.Application.Features.Product.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductResponseDto?>
    {
        public long Id { get; set; }
    }
}

