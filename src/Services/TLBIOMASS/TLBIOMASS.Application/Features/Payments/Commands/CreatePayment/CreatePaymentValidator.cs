using FluentValidation;

namespace TLBIOMASS.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.PaymentCode)
            .NotEmpty().WithMessage("Payment Code is required.")
            .MaximumLength(50).WithMessage("Payment Code must not exceed 50 characters.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment Date is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one payment item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.WeighingTicketId).GreaterThan(0).WithMessage("Valid Weighing Ticket ID is required.");
            item.RuleFor(x => x.AgencyId).GreaterThan(0).When(x => x.AgencyId.HasValue).WithMessage("Valid Agency ID is required.");
            item.RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment Amount must be greater than 0.");
        });
    }
}
