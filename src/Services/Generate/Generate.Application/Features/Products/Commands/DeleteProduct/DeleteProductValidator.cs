using FluentValidation;

namespace Generate.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}

