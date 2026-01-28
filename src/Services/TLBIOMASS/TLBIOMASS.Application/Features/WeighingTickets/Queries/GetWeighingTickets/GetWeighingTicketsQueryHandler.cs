using MediatR;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Shared.DTOs.WeighingTicket;
using Shared.SeedWork;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Infrastructure.Persistences;
using TLBIOMASS.Domain.WeighingTickets.Specifications;
using TLBIOMASS.Domain.WeighingTickets;
using System.Linq;
using System.Collections.Generic;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTickets;

public class GetWeighingTicketsQueryHandler : IRequestHandler<GetWeighingTicketsQuery, PagedList<WeighingTicketResponseDto>>
{
    private readonly IWeighingTicketRepository _repository;
    private readonly TLBIOMASSContext _context;

    public GetWeighingTicketsQueryHandler(IWeighingTicketRepository repository, TLBIOMASSContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<PagedList<WeighingTicketResponseDto>> Handle(GetWeighingTicketsQuery request, CancellationToken cancellationToken)
    {
        // 1. Explicitly type as IQueryable to avoid IncludableQueryable assignment errors
        IQueryable<WeighingTicket> query = _repository.FindAll()
            .Include(x => x.FinalPayment)
            .Include(x => x.PaymentDetails);

        // 2. Exclude cancelled tickets
        var cancelledIds = _context.WeighingTicketCancels.Select(c => c.WeighingTicketId);
        query = query.Where(t => !cancelledIds.Contains(t.Id));

        // 3. Apply Specifications
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

        // 5. Apply Sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        // 6. Paginate Entities
        var pagedEntities = await _repository.GetPageAsync(
            query, 
            request.Filter.PageNumber, 
            request.Filter.PageSize, 
            cancellationToken);

        // 7. Map to DTOs
        var dtos = pagedEntities.Adapt<List<WeighingTicketResponseDto>>();

        // 8. Return PagedList of DTOs
        return new PagedList<WeighingTicketResponseDto>(
            dtos, 
            pagedEntities.GetMetaData().TotalItems, 
            request.Filter.PageNumber, 
            request.Filter.PageSize);
    }

    private IQueryable<WeighingTicket> ApplySorting(IQueryable<WeighingTicket> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "ticketnumber" => isDescending ? query.OrderByDescending(x => x.TicketNumber) : query.OrderBy(x => x.TicketNumber),
            "customername" => isDescending ? query.OrderByDescending(x => x.CustomerName) : query.OrderBy(x => x.CustomerName),
            "createddate" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
