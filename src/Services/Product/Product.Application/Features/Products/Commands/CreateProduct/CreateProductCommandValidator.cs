using FluentValidation;
using Shared.DTOs.Product;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(100).WithMessage("SKU must not exceed 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.ComparePrice)
                .GreaterThan(x => x.Price).WithMessage("Compare price must be greater than price")
                .When(x => x.ComparePrice.HasValue);

            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantValidator());
        }
    }

    public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantRequest>
    {
        public CreateProductVariantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Variant name is required")
                .MaximumLength(255).WithMessage("Variant name must not exceed 255 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("Variant SKU is required")
                .MaximumLength(100).WithMessage("Variant SKU must not exceed 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Variant price must be greater than 0");

            RuleFor(x => x.InventoryQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Inventory quantity must be greater than or equal to 0");

            RuleForEach(x => x.Attributes).SetValidator(new CreateProductVariantAttributeValidator());
        }
    }

    public class CreateProductVariantAttributeValidator : AbstractValidator<CreateProductVariantAttributeRequest>
    {
        public CreateProductVariantAttributeValidator()
        {
            RuleFor(x => x.AttributeDefId)
                .GreaterThan(0).WithMessage("AttributeDef ID is required");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Attribute value is required")
                .MaximumLength(500).WithMessage("Attribute value must not exceed 500 characters");
        }
    }
}