using Shared.DTOs.Category;
using Generate.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace Generate.Application.Features.Category.Queries.GetCategories
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponseDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryResponseDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.FindAll().ToListAsync(cancellationToken);
            return categories.Adapt<List<CategoryResponseDto>>();
        }
    }
}

