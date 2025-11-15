using AutoMapper;
using Product.Infrastructure.Interfaces;
using Shared.DTOs.Product;
using MediatR;

namespace Product.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetProductWithVariantsAsync(request.Id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
    }
}