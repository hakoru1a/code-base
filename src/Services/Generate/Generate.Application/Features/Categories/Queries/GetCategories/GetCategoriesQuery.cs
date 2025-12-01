using Shared.DTOs.Category;
using MediatR;

namespace Generate.Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesQuery : IRequest<List<CategoryResponseDto>>
    {
    }
}

