using MediatR;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.WeighingTickets.Commands.AssignReceiver;

public class AssignReceiverCommandHandler : IRequestHandler<AssignReceiverCommand, bool>
{
    private readonly IWeighingTicketRepository _weighingTicketRepository;
    private readonly IReceiverRepository _receiverRepository;

    public AssignReceiverCommandHandler(
        IWeighingTicketRepository weighingTicketRepository,
        IReceiverRepository receiverRepository)
    {
        _weighingTicketRepository = weighingTicketRepository;
        _receiverRepository = receiverRepository;
    }

    public async Task<bool> Handle(AssignReceiverCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _weighingTicketRepository.GetByIdAsync(request.WeighingTicketId);
        if (ticket == null)
            throw new NotFoundException("WeighingTicket", request.WeighingTicketId);

        var receiver = await _receiverRepository.GetByIdAsync(request.ReceiverId);
        if (receiver == null)
            throw new NotFoundException("Receiver", request.ReceiverId);

        ticket.AssignReceiver(request.ReceiverId);
        await _weighingTicketRepository.UpdateAndSaveAsync(ticket, cancellationToken);

        return true;
    }
}
