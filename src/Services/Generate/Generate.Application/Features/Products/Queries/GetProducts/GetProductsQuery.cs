using Shared.DTOs.Product;
using MediatR;

namespace Generate.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<List<ProductResponseDto>>
    {
    }
}

