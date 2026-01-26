using FluentValidation;

namespace TLBIOMASS.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TaxCode)
            .MaximumLength(50).WithMessage("Tax code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxCode));
    }
}
