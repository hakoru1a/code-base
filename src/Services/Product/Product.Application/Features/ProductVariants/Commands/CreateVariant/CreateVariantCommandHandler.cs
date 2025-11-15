using AutoMapper;
using Product.Infrastructure.Interfaces;
using Product.Domain.Entities;
using Shared.DTOs.Product;
using MediatR;

namespace Product.Application.Features.ProductVariants.Commands.CreateVariant
{
    public class CreateVariantCommandHandler : IRequestHandler<CreateVariantCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantAttributeRepository _attributeRepository;
        private readonly IMapper _mapper;

        public CreateVariantCommandHandler(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository,
            IProductVariantAttributeRepository attributeRepository,
            IMapper mapper)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _attributeRepository = attributeRepository;
            _mapper = mapper;
        }

        public async Task<ProductVariantDto> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {request.ProductId} not found");
            }

            // Create variant
            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Name = request.Name,
                SKU = request.SKU,
                Price = request.Price,
                ComparePrice = request.ComparePrice,
                InventoryQuantity = request.InventoryQuantity,
                TrackInventory = request.TrackInventory,
                Status = ProductVariantStatus.Active
            };

            var createdVariantId = await _variantRepository.CreateAsync(variant);
            await _variantRepository.SaveChangesAsync();

            // Create variant attributes if any
            if (request.Attributes?.Any() == true)
            {
                foreach (var attrRequest in request.Attributes)
                {
                    var attribute = new ProductVariantAttribute
                    {
                        ProductVariantId = createdVariantId,
                        AttributeDefId = attrRequest.AttributeDefId,
                        Value = attrRequest.Value
                    };

                    await _attributeRepository.CreateAsync(attribute);
                }
                await _attributeRepository.SaveChangesAsync();
            }

            // Get the complete variant with attributes
            var resultVariant = await _variantRepository.GetVariantWithAttributesAsync(createdVariantId);
            return _mapper.Map<ProductVariantDto>(resultVariant);
        }
    }
}