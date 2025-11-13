using Shared.DTOs.Product;
using MediatR;

namespace Generate.Application.Features.Product.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<List<ProductResponseDto>>
    {
    }
}

