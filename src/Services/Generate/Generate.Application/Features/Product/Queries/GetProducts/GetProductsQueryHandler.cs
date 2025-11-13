using AutoMapper;
using Shared.DTOs.Product;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Product.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.FindAll()
                .Include(p => p.Category)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ProductResponseDto>>(products);
        }
    }
}

