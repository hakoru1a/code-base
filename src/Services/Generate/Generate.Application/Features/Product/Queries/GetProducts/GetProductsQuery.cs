using Generate.Application.Common.DTOs.Product;
using MediatR;

namespace Generate.Application.Features.Product.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<List<ProductResponseDto>>
    {
    }
}

