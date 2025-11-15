using AutoMapper;
using Product.Infrastructure.Interfaces;
using Shared.DTOs.Product;
using Product.Domain.Entities;
using MediatR;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductVariantAttributeRepository _productVariantAttributeRepository;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository,
            IProductVariantAttributeRepository productVariantAttributeRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _productVariantAttributeRepository = productVariantAttributeRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Domain.Entities.Product>(request);

            var createdProduct = await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            // Create variants if any
            if (request.Variants?.Any() == true)
            {
                foreach (var variantRequest in request.Variants)
                {
                    var variant = _mapper.Map<ProductVariant>(variantRequest);
                    variant.ProductId = createdProduct;

                    var createdVariant = await _productVariantRepository.CreateAsync(variant);
                    await _productVariantRepository.SaveChangesAsync();

                    // Create variant attributes if any
                    if (variantRequest.Attributes?.Any() == true)
                    {
                        foreach (var attrRequest in variantRequest.Attributes)
                        {
                            var attribute = _mapper.Map<ProductVariantAttribute>(attrRequest);
                            attribute.ProductVariantId = createdVariant;

                            await _productVariantAttributeRepository.CreateAsync(attribute);
                        }
                        await _productVariantAttributeRepository.SaveChangesAsync();
                    }
                }
            }

            // Get the complete product with variants and attributes
            var resultProduct = await _productRepository.GetProductWithVariantsAsync(createdProduct);
            return _mapper.Map<ProductDto>(resultProduct);
        }
    }
}