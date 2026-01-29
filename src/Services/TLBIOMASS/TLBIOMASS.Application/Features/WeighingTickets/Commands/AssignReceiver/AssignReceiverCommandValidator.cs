using FluentValidation;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.AssignReceiver;

public class AssignReceiverCommandValidator : AbstractValidator<AssignReceiverCommand>
{
    public AssignReceiverCommandValidator()
    {
        RuleFor(x => x.WeighingTicketId)
            .GreaterThan(0).WithMessage("WeighingTicketId is required.");

        RuleFor(x => x.ReceiverId)
            .GreaterThan(0).WithMessage("ReceiverId is required.");
    }
}
