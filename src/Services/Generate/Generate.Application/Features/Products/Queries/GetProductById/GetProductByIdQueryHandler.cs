using Mapster;
using Shared.DTOs.Product;
using Generate.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponseDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.FindAll()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            return product?.Adapt<ProductResponseDto>() ?? throw new KeyNotFoundException($"Product with ID {request.Id} not found");
        }
    }
}

