using MediatR;
using Shared.DTOs.Product;

namespace Product.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        public long Id { get; set; }

        public GetProductByIdQuery(long id)
        {
            Id = id;
        }
    }
}