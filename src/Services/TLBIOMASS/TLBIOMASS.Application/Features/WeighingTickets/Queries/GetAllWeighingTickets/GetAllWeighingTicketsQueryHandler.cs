using MediatR;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Shared.DTOs.WeighingTicket;
using Microsoft.EntityFrameworkCore;
using Mapster;
using TLBIOMASS.Infrastructure.Persistences;
using TLBIOMASS.Domain.WeighingTickets.Specifications;
using TLBIOMASS.Domain.WeighingTickets;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetAllWeighingTickets;

public class GetAllWeighingTicketsQueryHandler : IRequestHandler<GetAllWeighingTicketsQuery, List<WeighingTicketResponseDto>>
{
    private readonly IWeighingTicketRepository _repository;
    private readonly TLBIOMASSContext _context;

    public GetAllWeighingTicketsQueryHandler(IWeighingTicketRepository repository, TLBIOMASSContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<WeighingTicketResponseDto>> Handle(GetAllWeighingTicketsQuery request, CancellationToken cancellationToken)
    {
        // 1. Start with base query
        IQueryable<WeighingTicket> query = _repository.FindAll()
            .Include(x => x.FinalPayment)
            .Include(x => x.PaymentDetails);
        
        // 2. Exclude cancelled tickets
        var cancelledIds = _context.WeighingTicketCancels.Select(c => c.WeighingTicketId);
        query = query.Where(t => !cancelledIds.Contains(t.Id));

        // 3. Apply Filters (Essential for specific UI needs)
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var spec = new WeighingTicketSearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == request.Filter.CustomerId.Value);
        }

        if (request.Filter.MaterialId.HasValue)
        {
            query = query.Where(x => x.MaterialId == request.Filter.MaterialId.Value);
        }

        if (request.Filter.TicketType != null)
        {
            var spec = new WeighingTicketTypeSpecification(request.Filter.TicketType);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsCompleted.HasValue)
        {
            var spec = new WeighingTicketIsCompletedSpecification(request.Filter.IsCompleted.Value);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.FromDate.HasValue || request.Filter.ToDate.HasValue)
        {
            var spec = new WeighingTicketDateRangeSpecification(request.Filter.FromDate, request.Filter.ToDate);
            query = query.Where(spec.ToExpression());
        }

        // 4. Payment-specific filters
        if (request.Filter.IsPaid.HasValue)
        {
            if (request.Filter.IsPaid.Value)
                query = query.Where(x => x.PaymentDetails.Any());
            else
                query = query.Where(x => !x.PaymentDetails.Any());
        }

        if (request.Filter.IsFullyPaid.HasValue)
        {
            if (request.Filter.IsFullyPaid.Value)
                query = query.Where(x => x.PaymentDetails.Any(pd => pd.RemainingAmount == 0));
            else
                query = query.Where(x => !x.PaymentDetails.Any(pd => pd.RemainingAmount == 0));
        }

        // 5. Fetch and Adapt (No Sorting needed as per user request)
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<WeighingTicketResponseDto>>();
    }
}
