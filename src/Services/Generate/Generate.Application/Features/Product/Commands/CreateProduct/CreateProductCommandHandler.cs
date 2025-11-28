using Generate.Domain.Repositories;
using MediatR;

namespace Generate.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, long>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Generate.Domain.Entities.Categories.Category? category = null;
            if (request.CategoryId.HasValue)
            {
                category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            }

            // Use DDD factory method
            var product = Generate.Domain.Entities.Products.Product.Create(request.Name, category);

            var result = await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            return result;
        }
    }
}

