using Shared.DTOs.Category;
using MediatR;

namespace Generate.Application.Features.Category.Queries.GetCategories
{
    public class GetCategoriesQuery : IRequest<List<CategoryResponseDto>>
    {
    }
}

