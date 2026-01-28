using Mapster;
using Shared.DTOs.Category;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.SeedWork;
using Generate.Domain.Categories.Interfaces;

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

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(c => c.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerms))
            {
                query = query.Where(c => c.Name.Contains(filter.SearchTerms));
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(c => c.CreatedDate >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(c => c.CreatedDate <= filter.CreatedTo.Value);
            }


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

