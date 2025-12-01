using Generate.Domain.Categories;
using MediatR;
namespace Generate.Application.Features.Categories.Commands.CreateCategory
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
            var category = Category.Create(request.Name);

            var result = await _categoryRepository.CreateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return result;
        }
    }
}
