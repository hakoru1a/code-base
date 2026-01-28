using MediatR;
using Shared.DTOs.Payment;
using Shared.SeedWork;
using TLBIOMASS.Domain.Payments.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Payments;
using System.Linq;
using System.Collections.Generic;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetails;

public class GetPaymentDetailsQueryHandler : IRequestHandler<GetPaymentDetailsQuery, PagedList<PaymentDetailResponseDto>>
{
    private readonly IPaymentDetailRepository _repository;

    public GetPaymentDetailsQueryHandler(IPaymentDetailRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<PaymentDetailResponseDto>> Handle(GetPaymentDetailsQuery request, CancellationToken cancellationToken)
    {
        // 1. Explicitly type as IQueryable
        IQueryable<PaymentDetail> query = _repository.FindAll()
            .Include(x => x.WeighingTicket)
            .Include(x => x.Agency);

        // 2. Apply Filters
        if (request.Filter.AgencyId.HasValue)
        {
            query = query.Where(x => x.AgencyId == request.Filter.AgencyId.Value);
        }

        if (request.Filter.FromDate.HasValue)
        {
            query = query.Where(x => x.PaymentDate >= request.Filter.FromDate.Value);
        }

        if (request.Filter.ToDate.HasValue)
        {
            query = query.Where(x => x.PaymentDate <= request.Filter.ToDate.Value);
        }

        if (request.Filter.IsPaid.HasValue)
        {
            query = query.Where(x => x.IsPaid == request.Filter.IsPaid.Value);
        }

        if (request.Filter.IsLocked.HasValue)
        {
            query = query.Where(x => x.IsLocked == request.Filter.IsLocked.Value);
        }

        // 3. Apply Sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        // 4. Paginate Entities
        var pagedEntities = await _repository.GetPageAsync(
            query,
            request.Filter.PageNumber,
            request.Filter.PageSize,
            cancellationToken);

        // 5. Map to DTOs
        var dtos = pagedEntities.Adapt<List<PaymentDetailResponseDto>>();

        // 6. Return
        return new PagedList<PaymentDetailResponseDto>(
            dtos,
            pagedEntities.GetMetaData().TotalItems,
            request.Filter.PageNumber,
            request.Filter.PageSize);
    }

    private IQueryable<PaymentDetail> ApplySorting(IQueryable<PaymentDetail> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "paymentcode" => isDescending ? query.OrderByDescending(x => x.PaymentCode) : query.OrderBy(x => x.PaymentCode),
            "paymentdate" => isDescending ? query.OrderByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentDate),
            "amount" => isDescending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "createddate" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
