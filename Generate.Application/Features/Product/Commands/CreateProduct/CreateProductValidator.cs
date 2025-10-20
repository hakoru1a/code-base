using FluentValidation;

namespace Generate.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .NotNull().WithMessage("Name cannot be null")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required")
                .NotNull().WithMessage("CategoryId cannot be null")
                .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        }
    }
}

