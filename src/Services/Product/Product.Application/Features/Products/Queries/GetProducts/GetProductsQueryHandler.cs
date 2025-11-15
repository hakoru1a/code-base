using AutoMapper;
using Product.Infrastructure.Interfaces;
using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Product;

namespace Product.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetProductsWithVariantsAsync();

            // Apply filters
            var query = products.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        p.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => (int)p.Status == request.Status.Value);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

            // Apply pagination
            var pagedProducts = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts);

            return PagedResult<ProductDto>.Create(productDtos.ToList(), totalItems, request.Page, request.PageSize);
        }
    }
}