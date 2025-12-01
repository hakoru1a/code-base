using Shared.DTOs.Category;
using MediatR;

namespace Generate.Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQuery : IRequest<CategoryResponseDto>
    {
        public long Id { get; set; }
    }
}

