using Mapster;
using Shared.DTOs.Product;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace Generate.Application.Features.Product.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetProductsQueryHandler>? _logger;

        public GetProductsQueryHandler(
            IProductRepository productRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetProductsQueryHandler>? logger = null)
        {
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Generate.Domain.Entities.Product> query = _productRepository.FindAll().Include(p => p.Category);

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
        /// NOTE: Current Product entity only has Name and Category.
        /// Price, Department, Brand filters are examples for future enhancement.
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
                _logger?.LogDebug("User has CanViewAll privilege - bypassing all filters");
                return query;
            }

            // Apply category restrictions based on permissions
            if (filterContext.AllowedCategories?.Any() == true)
            {
                _logger?.LogDebug("Applying category filter: {Categories}", 
                    string.Join(", ", filterContext.AllowedCategories));
                    
                query = query.Where(p => p.Category != null && 
                    filterContext.AllowedCategories.Contains(p.Category.Name));
            }

            // TODO: Price filter - requires adding Price property to Product entity
            // if (filterContext.MaxPrice.HasValue)
            // {
            //     query = query.Where(p => p.Price <= filterContext.MaxPrice.Value);
            // }

            // TODO: Department filter - requires adding Department property to Product entity
            // if (!string.IsNullOrEmpty(filterContext.DepartmentFilter) && !filterContext.CanViewCrossDepartment)
            // {
            //     query = query.Where(p => p.Department == filterContext.DepartmentFilter);
            // }

            // TODO: Brand filter - requires adding Brand property to Product entity
            // if (filterContext.AllowedBrands?.Any() == true)
            // {
            //     query = query.Where(p => filterContext.AllowedBrands.Contains(p.Brand));
            // }

            // TODO: Region filter - requires adding Region property to Product entity
            // if (!string.IsNullOrEmpty(filterContext.RegionFilter))
            // {
            //     query = query.Where(p => p.Region == filterContext.RegionFilter);
            // }

            // Log applied filters for debugging
            _logger?.LogInformation(
                "Applied product filters - CanViewAll: {CanViewAll}, CategoryCount: {CategoryCount}",
                filterContext.CanViewAll,
                filterContext.AllowedCategories?.Count ?? 0);

            return query;
        }
    }
}

