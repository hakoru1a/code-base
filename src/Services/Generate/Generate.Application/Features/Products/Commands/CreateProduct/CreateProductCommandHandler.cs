using Generate.Domain.Products;
using Generate.Domain.Categories;

using MediatR;
using Generate.Domain.Categories.Interfaces;
using Generate.Domain.Products.Interfaces;

namespace Generate.Application.Features.Products.Commands.CreateProduct
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
            Category? category = null;
            if (request.CategoryId.HasValue)
            {
                category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            }

            // Use DDD factory method
            var product = Product.Create(request.Name, category);

            var result = await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            return result;
        }
    }
}

