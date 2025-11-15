using AutoMapper;
using Product.Infrastructure.Interfaces;
using Shared.DTOs.Product;
using MediatR;

namespace Product.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {request.Id} not found");
            }

            // Map update properties
            _mapper.Map(request, existingProduct);

            await _productRepository.UpdateAsync(existingProduct);
            await _productRepository.SaveChangesAsync();

            // Get the updated product with variants
            var updatedProduct = await _productRepository.GetProductWithVariantsAsync(request.Id);
            return _mapper.Map<ProductDto>(updatedProduct);
        }
    }
}