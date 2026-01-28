using MediatR;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Shared.DTOs.WeighingTicket;
using Microsoft.EntityFrameworkCore;
using Mapster;
using TLBIOMASS.Infrastructure.Persistences;
using TLBIOMASS.Domain.WeighingTickets;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTickets;

public class GetWeighingTicketsQueryHandler : IRequestHandler<GetWeighingTicketsQuery, List<WeighingTicketResponseDto>>
{
    private readonly IWeighingTicketRepository _repository;
    private readonly TLBIOMASSContext _context;

    public GetWeighingTicketsQueryHandler(IWeighingTicketRepository repository, TLBIOMASSContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<WeighingTicketResponseDto>> Handle(GetWeighingTicketsQuery request, CancellationToken cancellationToken)
    {
        // 1. Start with base query
        IQueryable<WeighingTicket> query = _repository.FindAll()
            .Include(x => x.FinalPayment)
            .Include(x => x.PaymentDetails);
        
        // 2. Exclude cancelled tickets
        var cancelledIds = _context.WeighingTicketCancels.Select(c => c.WeighingTicketId);
        query = query.Where(t => !cancelledIds.Contains(t.Id));

        // 3. Apply Filters
        if (!string.IsNullOrWhiteSpace(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(x => x.TicketNumber.ToLower().Contains(search) || 
                               (x.VehiclePlate != null && x.VehiclePlate.ToLower().Contains(search)) ||
                               (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)));
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
            query = query.Where(x => x.TicketType == request.Filter.TicketType);
        }

        if (request.Filter.IsCompleted.HasValue)
        {
            if (request.Filter.IsCompleted.Value)
                query = query.Where(x => x.Weights != null && x.Weights.NetWeight > 0);
            else
                query = query.Where(x => x.Weights == null || x.Weights.NetWeight == 0);
        }

        if (request.Filter.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate.Date >= request.Filter.FromDate.Value.Date);
        }

        if (request.Filter.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate.Date <= request.Filter.ToDate.Value.Date);
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
                query = query.Where(x => x.PaymentDetails.Any(pd => pd.PaymentAmount.RemainingAmount == 0));
            else
                query = query.Where(x => !x.PaymentDetails.Any(pd => pd.PaymentAmount.RemainingAmount == 0));
        }

        // 5. Fetch and Adapt (No Sorting needed as per user request)
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<WeighingTicketResponseDto>>();
    }
}

