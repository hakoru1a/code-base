using MediatR;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Shared.DTOs.WeighingTicket;
using Contracts.Exceptions;
using Mapster;
using TLBIOMASS.Infrastructure.Persistences;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTicketById;

public class GetWeighingTicketByIdQueryHandler : IRequestHandler<GetWeighingTicketByIdQuery, WeighingTicketResponseDto>
{
    private readonly IWeighingTicketRepository _repository;
    private readonly TLBIOMASSContext _context;

    public GetWeighingTicketByIdQueryHandler(IWeighingTicketRepository repository, TLBIOMASSContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<WeighingTicketResponseDto> Handle(GetWeighingTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var weighingTicket = await _repository.GetByIdAsync(request.Id);
        
        if (weighingTicket == null)
            throw new NotFoundException("WeighingTicket", request.Id);

        // Check if cancelled
        var isCancelled = await _context.WeighingTicketCancels.AnyAsync(c => c.WeighingTicketId == request.Id, cancellationToken);
        if (isCancelled)
             throw new NotFoundException("WeighingTicket", request.Id);

        // Fetch payment info
        var payment = await _context.WeighingTicketPayments
            .FirstOrDefaultAsync(p => p.WeighingTicketId == request.Id, cancellationToken);

        var latestPayment = await _context.PaymentDetails
            .Where(pd => pd.WeighingTicketId == request.Id)
            .OrderByDescending(pd => pd.CreatedDate)
            .ThenByDescending(pd => pd.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = weighingTicket.Adapt<WeighingTicketResponseDto>();
        
        // Map payment info
        dto.FinalUnitPrice = payment?.UnitPrice;
        dto.FinalTotalAmount = payment?.TotalPayableAmount;
        dto.RemainingAmount = latestPayment?.PaymentAmount.RemainingAmount ?? (payment?.TotalPayableAmount ?? (decimal)weighingTicket.TotalAmount);
        dto.IsPaid = latestPayment != null;
        dto.IsFullyPaid = latestPayment != null && latestPayment.PaymentAmount.RemainingAmount == 0;
        dto.LastModifiedDate = weighingTicket.UpdatedAt;

        return dto;
    }
}
