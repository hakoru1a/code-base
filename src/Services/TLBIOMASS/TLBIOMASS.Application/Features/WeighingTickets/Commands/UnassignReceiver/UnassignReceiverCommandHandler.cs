using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.UnassignReceiver;

public class UnassignReceiverCommandHandler : IRequestHandler<UnassignReceiverCommand, ApiResult<int>>
{
    private readonly IWeighingTicketRepository _weighingTicketRepository;

    public UnassignReceiverCommandHandler(IWeighingTicketRepository weighingTicketRepository)
    {
        _weighingTicketRepository = weighingTicketRepository;
    }

    public async Task<ApiResult<int>> Handle(UnassignReceiverCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _weighingTicketRepository.GetByIdAsync(request.WeighingTicketId);
        if (ticket == null)
        {
            return new ApiErrorResult<int>("Weighing ticket not found.");
        }

        ticket.UnassignReceiver();
        await _weighingTicketRepository.UpdateAndSaveAsync(ticket, cancellationToken);

        return new ApiSuccessResult<int>(ticket.Id, "Receiver unassigned successfully.");
    }
}
