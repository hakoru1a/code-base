using FluentValidation;

namespace Generate.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .NotNull().WithMessage("Name cannot be null")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");
        }
    }
}

