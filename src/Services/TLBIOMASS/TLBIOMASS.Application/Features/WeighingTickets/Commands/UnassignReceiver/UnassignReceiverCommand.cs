using MediatR;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.UnassignReceiver;

public class UnassignReceiverCommand : IRequest<ApiResult<int>>
{
    public int WeighingTicketId { get; set; }
}
