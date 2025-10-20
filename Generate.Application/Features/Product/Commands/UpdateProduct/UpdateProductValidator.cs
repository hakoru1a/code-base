using FluentValidation;

namespace Generate.Application.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");

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

