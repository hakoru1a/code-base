using MediatR;
using Shared.DTOs.Payment;
using Shared.SeedWork;
using TLBIOMASS.Domain.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TLBIOMASS.Domain.Payments.Specifications;

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
            var spec = new PaymentDetailSearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.AgencyId.HasValue)
        {
            var spec = new PaymentDetailAgencySpecification(request.Filter.AgencyId.Value);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.FromDate.HasValue || request.Filter.ToDate.HasValue)
        {
            var spec = new PaymentDetailDateRangeSpecification(request.Filter.FromDate, request.Filter.ToDate);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsPaid.HasValue)
        {
            query = query.Where(x => x.IsPaid == request.Filter.IsPaid.Value);
        }

        if (request.Filter.IsLocked.HasValue)
        {
            query = query.Where(x => x.IsLocked == request.Filter.IsLocked.Value);
        }

        // 2. Grouping
        var groupingQuery = query.GroupBy(x => new { x.PaymentCode, x.PaymentDate, x.AgencyId, AgencyName = x.Agency.Name })
            .Select(g => new PaymentGroupResponseDto
            {
                PaymentCode = g.Key.PaymentCode,
                PaymentDate = g.Key.PaymentDate,
                AgencyId = g.Key.AgencyId,
                AgencyName = g.Key.AgencyName,
                TotalAmount = g.Sum(x => x.Amount),
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
