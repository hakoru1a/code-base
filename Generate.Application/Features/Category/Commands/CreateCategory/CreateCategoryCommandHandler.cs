using Generate.Domain.Interfaces;
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
            var category = new Domain.Entities.Category
            {
                Name = request.Name
            };

            var result = await _categoryRepository.CreateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return result;
        }
    }
}
