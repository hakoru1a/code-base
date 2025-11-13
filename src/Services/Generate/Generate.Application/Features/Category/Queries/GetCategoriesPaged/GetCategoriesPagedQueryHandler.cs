using AutoMapper;
using Shared.DTOs.Category;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.SeedWork;

namespace Generate.Application.Features.Category.Queries.GetCategoriesPaged
{
    public class GetCategoriesPagedQueryHandler : IRequestHandler<GetCategoriesPagedQuery, PagedList<CategoryResponseDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoriesPagedQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
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

            // Apply sorting
            query = ApplySorting(query, filter.OrderBy, filter.OrderByDirection);

            // Get paginated results
            var pagedCategories = await _categoryRepository.GetPageAsync(
                query,
                filter.PageNumber,
                filter.PageSize,
                cancellationToken);

            // Map to DTOs
            var categoryDtos = _mapper.Map<List<CategoryResponseDto>>(pagedCategories);

            return new PagedList<CategoryResponseDto>(
                categoryDtos,
                pagedCategories.GetMetaData().TotalItems,
                filter.PageNumber,
                filter.PageSize);
        }

        private IQueryable<Generate.Domain.Entities.Category> ApplySorting(
            IQueryable<Generate.Domain.Entities.Category> query,
            string orderBy,
            string orderByDirection)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return query.OrderByDescending(c => c.CreatedDate);
            }

            var isDescending = orderByDirection?.ToLower() == "desc";

            return orderBy.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "createdate" or "created" => isDescending ? query.OrderByDescending(c => c.CreatedDate) : query.OrderBy(c => c.CreatedDate),
                "lastmodifieddate" or "modified" => isDescending ? query.OrderByDescending(c => c.LastModifiedDate) : query.OrderBy(c => c.LastModifiedDate),
                _ => query.OrderByDescending(c => c.CreatedDate)
            };
        }
    }
}

