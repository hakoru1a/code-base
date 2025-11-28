using Generate.Domain.Repositories;
using Generate.Domain.Entities.Categories;
using MediatR;

namespace Generate.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, long>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<long> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Use DDD factory method
            var category = Domain.Entities.Categories.Category.Create(request.Name);

            var result = await _categoryRepository.CreateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return result;
        }
    }
}
