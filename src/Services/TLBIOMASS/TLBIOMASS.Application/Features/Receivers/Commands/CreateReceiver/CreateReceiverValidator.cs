using FluentValidation;

namespace TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver
{
    public class CreateReceiverValidator : AbstractValidator<CreateReceiverCommand>
    {
        public CreateReceiverValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(50).WithMessage("Phone must not exceed 50 characters");

            RuleFor(x => x.BankAccount)
                .MaximumLength(100).WithMessage("Bank account must not exceed 100 characters");

            RuleFor(x => x.BankName)
                .MaximumLength(255).WithMessage("Bank name must not exceed 255 characters");

            RuleFor(x => x.IdentityNumber)
                .MaximumLength(20).WithMessage("Identity number must not exceed 20 characters");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters");
        }
    }
}
