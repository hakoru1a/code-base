using Shared.DTOs.Category;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.Categories.Queries.GetCategoriesPaged
{
    /// <summary>
    /// Query to get paginated list of categories
    /// </summary>
    public class GetCategoriesPagedQuery : IRequest<PagedList<CategoryResponseDto>>
    {
        public CategoryFilterDto Filter { get; set; } = new();
    }
}

