using MediatR;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.CreateWeighingTicketCancel;

public class CreateWeighingTicketCancelCommand : IRequest<long>
{
    public int WeighingTicketId { get; set; }
    public string? CancelReason { get; set; }
}
