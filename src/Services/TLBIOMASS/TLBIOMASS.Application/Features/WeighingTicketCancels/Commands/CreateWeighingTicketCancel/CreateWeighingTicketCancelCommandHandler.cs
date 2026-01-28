using MediatR;
using TLBIOMASS.Domain.WeighingTicketCancels;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.CreateWeighingTicketCancel;

public class CreateWeighingTicketCancelCommandHandler : IRequestHandler<CreateWeighingTicketCancelCommand, long>
{
    private readonly IWeighingTicketCancelRepository _cancelRepository;
    private readonly IWeighingTicketRepository _ticketRepository; // To verify existence if needed

    public CreateWeighingTicketCancelCommandHandler(
        IWeighingTicketCancelRepository cancelRepository,
        IWeighingTicketRepository ticketRepository)
    {
        _cancelRepository = cancelRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<long> Handle(CreateWeighingTicketCancelCommand request, CancellationToken cancellationToken)
    {
    
        var ticket = await _ticketRepository.GetByIdAsync(request.WeighingTicketId);
        if (ticket == null)
             throw new NotFoundException("WeighingTicket", request.WeighingTicketId);
        
        var isCancelled = await _cancelRepository.FindAll()
            .AnyAsync(x => x.WeighingTicketId == request.WeighingTicketId, cancellationToken);
        
        var cancelEntity = WeighingTicketCancel.Create(request.WeighingTicketId, request.CancelReason, isCancelled);
        
        await _cancelRepository.CreateAsync(cancelEntity);
        await _cancelRepository.SaveChangesAsync(cancellationToken);

        return cancelEntity.Id;
    }
}
