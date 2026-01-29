using MediatR;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.AssignReceiver;

public class AssignReceiverCommand : IRequest<bool>
{
    public int WeighingTicketId { get; set; }
    public int ReceiverId { get; set; }
}
