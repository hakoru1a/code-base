using Mapster;
using Shared.DTOs.Product;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Infrastructure.Extensions;

namespace Generate.Application.Features.Product.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetProductsQueryHandler(
            IProductRepository productRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository.FindAll().Include(p => p.Category);

            // Apply filter context from PRODUCT:VIEW policy
            var filterContext = _httpContextAccessor.HttpContext?.GetProductFilterContext();
            if (filterContext != null)
            {
                query = ApplyPolicyFilters(query, filterContext);
            }

            var products = await query.ToListAsync(cancellationToken);
            return products.Adapt<List<ProductResponseDto>>();
        }

        /// <summary>
        /// Apply policy-based filters to product query
        /// </summary>
        /// <param name="query">Product query</param>
        /// <param name="filterContext">Filter context from policy evaluation</param>
        /// <returns>Filtered query</returns>
        private IQueryable<Generate.Domain.Entities.Product> ApplyPolicyFilters(
            IQueryable<Generate.Domain.Entities.Product> query, 
            ProductFilterContext filterContext)
        {
            // Admin/Manager can view all products - bypass all filters
            if (filterContext.CanViewAll)
            {
                return query;
            }

            // Apply price range filter from JWT claims
            if (filterContext.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filterContext.MaxPrice.Value);
            }

            if (filterContext.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filterContext.MinPrice.Value);
            }

            // Apply category restrictions
            if (filterContext.AllowedCategories?.Any() == true)
            {
                query = query.Where(p => filterContext.AllowedCategories.Contains(p.Category.Name));
            }

            // Apply department filter
            if (!string.IsNullOrEmpty(filterContext.DepartmentFilter) && !filterContext.CanViewCrossDepartment)
            {
                // Assuming products have a Department field, adjust as needed
                // query = query.Where(p => p.Department == filterContext.DepartmentFilter);
            }

            // Apply region filter
            if (!string.IsNullOrEmpty(filterContext.RegionFilter))
            {
                // Assuming products have a Region field, adjust as needed
                // query = query.Where(p => p.Region == filterContext.RegionFilter);
            }

            // Apply brand restrictions
            if (filterContext.AllowedBrands?.Any() == true)
            {
                // Assuming products have a Brand field, adjust as needed
                // query = query.Where(p => filterContext.AllowedBrands.Contains(p.Brand));
            }

            // Filter discontinued products if not allowed
            if (!filterContext.IncludeDiscontinued)
            {
                // Assuming products have an IsActive or Status field
                // query = query.Where(p => p.IsActive == true);
            }

            return query;
        }
    }
}

