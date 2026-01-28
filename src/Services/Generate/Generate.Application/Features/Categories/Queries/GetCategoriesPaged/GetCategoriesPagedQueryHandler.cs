using Mapster;
using Shared.DTOs.Category;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.SeedWork;
using Generate.Domain.Categories.Interfaces;
using Shared.Extensions;

namespace Generate.Application.Features.Categories.Queries.GetCategoriesPaged
{
    public class GetCategoriesPagedQueryHandler : IRequestHandler<GetCategoriesPagedQuery, PagedList<CategoryResponseDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoriesPagedQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<PagedList<CategoryResponseDto>> Handle(GetCategoriesPagedQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;

            // Start with base query
            var query = _categoryRepository.FindAll(trackChanges: false);

            // Apply SearchTerms (custom logic cho Category)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerms))
            {
                query = query.Where(c => c.Name.Contains(filter.SearchTerms));
            }

            // Apply filters tự động từ DTO
            query = query.ApplyFilters(filter);

            // Apply sorting
            query = query.ApplySort(filter.OrderBy, filter.OrderByDirection);

            // Get paginated results
            var pagedCategories = await _categoryRepository.GetPageAsync(
                query,
                filter.PageNumber,
                filter.PageSize,
                cancellationToken);

            // Map to DTOs
            var categoryDtos = pagedCategories.Adapt<List<CategoryResponseDto>>();

            return new PagedList<CategoryResponseDto>(
                categoryDtos,
                pagedCategories.GetMetaData().TotalItems,
                filter.PageNumber,
                filter.PageSize);
        }

    }
}


