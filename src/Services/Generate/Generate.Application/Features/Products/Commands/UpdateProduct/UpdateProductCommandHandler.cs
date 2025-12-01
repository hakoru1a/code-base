using Generate.Domain.Products;
using Generate.Domain.Categories;
using MediatR;

namespace Generate.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);

            if (product == null)
                return false;

            // Use business method for name update
            product.UpdateName(request.Name);

            // Handle category assignment using business methods
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category != null)
                {
                    product.AssignToCategory(category);
                }
            }
            else
            {
                product.RemoveFromCategory();
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }
    }
}

