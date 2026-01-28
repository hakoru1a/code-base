using MediatR;
using Shared.DTOs.Payment;
using Shared.SeedWork;
using TLBIOMASS.Domain.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentGroups;

public class GetPaymentGroupsQueryHandler : IRequestHandler<GetPaymentGroupsQuery, PagedList<PaymentGroupResponseDto>>
{
    private readonly IPaymentDetailRepository _repository;

    public GetPaymentGroupsQueryHandler(IPaymentDetailRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<PaymentGroupResponseDto>> Handle(GetPaymentGroupsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Filters
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(x => x.Info.PaymentCode.ToLower().Contains(search) || 
                               (x.Agency != null && x.Agency.Name.ToLower().Contains(search)) ||
                               (x.WeighingTicket != null && x.WeighingTicket.Receiver != null && x.WeighingTicket.Receiver.Name.ToLower().Contains(search)));
        }

        if (request.Filter.AgencyId.HasValue)
        {
            query = query.Where(x => x.AgencyId == request.Filter.AgencyId.Value);
        }

        if (request.Filter.FromDate.HasValue)
        {
            query = query.Where(x => x.Info.PaymentDate.Date >= request.Filter.FromDate.Value.Date);
        }

        if (request.Filter.ToDate.HasValue)
        {
            query = query.Where(x => x.Info.PaymentDate.Date <= request.Filter.ToDate.Value.Date);
        }

        if (request.Filter.IsPaid.HasValue)
        {
            query = query.Where(x => x.ProcessStatus.IsPaid == request.Filter.IsPaid.Value);
        }

        if (request.Filter.IsLocked.HasValue)
        {
            query = query.Where(x => x.ProcessStatus.IsLocked == request.Filter.IsLocked.Value);
        }

        // 2. Grouping
        var groupingQuery = query.GroupBy(x => new { x.Info.PaymentCode, x.Info.PaymentDate, x.AgencyId, AgencyName = x.Agency.Name })
            .Select(g => new PaymentGroupResponseDto
            {
                PaymentCode = g.Key.PaymentCode,
                PaymentDate = g.Key.PaymentDate,
                AgencyId = g.Key.AgencyId,
                AgencyName = g.Key.AgencyName,
                TotalAmount = g.Sum(x => x.PaymentAmount.Amount),
                TicketCount = g.Select(x => x.WeighingTicketId).Distinct().Count()
            });

        // 3. Apply Sorting
        groupingQuery = ApplySorting(groupingQuery, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var totalItems = await groupingQuery.CountAsync(cancellationToken);
        var items = await groupingQuery
            .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<PaymentGroupResponseDto>(items, totalItems, request.Filter.PageNumber, request.Filter.PageSize);
    }

    private IQueryable<PaymentGroupResponseDto> ApplySorting(IQueryable<PaymentGroupResponseDto> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentCode);
        }

        return sortBy.ToLower() switch
        {
            "paymentcode" => isDescending ? query.OrderByDescending(x => x.PaymentCode) : query.OrderBy(x => x.PaymentCode),
            "paymentdate" => isDescending ? query.OrderByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentDate),
            "totalamount" => isDescending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "ticketcount" => isDescending ? query.OrderByDescending(x => x.TicketCount) : query.OrderBy(x => x.TicketCount),
            _ => query.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentCode)
        };
    }
}
