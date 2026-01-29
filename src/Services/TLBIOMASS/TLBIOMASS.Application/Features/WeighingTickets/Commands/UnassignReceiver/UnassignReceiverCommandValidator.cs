using FluentValidation;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.UnassignReceiver;

public class UnassignReceiverCommandValidator : AbstractValidator<UnassignReceiverCommand>
{
    public UnassignReceiverCommandValidator()
    {
        RuleFor(x => x.WeighingTicketId)
            .GreaterThan(0).WithMessage("WeighingTicketId is required.");
    }
}
