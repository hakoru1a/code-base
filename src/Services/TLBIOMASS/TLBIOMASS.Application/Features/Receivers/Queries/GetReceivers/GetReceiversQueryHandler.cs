using System.Linq;
using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.Receivers.Specifications;
using Mapster;
using TLBIOMASS.Domain.Receivers;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQueryHandler : IRequestHandler<GetReceiversQuery, PagedList<ReceiverResponseDto>>
{
    private readonly IReceiverRepository _repository;

    public GetReceiversQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<ReceiverResponseDto>> Handle(GetReceiversQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        query = ApplyFilter(query, request.Filter);

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new ReceiverSearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new ReceiverIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<ReceiverResponseDto>(
            pagedItems.Adapt<List<ReceiverResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Receiver> ApplyFilter(IQueryable<Receiver> query, ReceiverPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        return query;
    }

    private static IQueryable<Receiver> ApplySort(IQueryable<Receiver> query, string? orderBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Id);

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            "phone" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Phone : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Phone : null),
            "email" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Email : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Email : null),
            "address" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Address : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Address : null),
            "bankname" => isDescending
                ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankName : null)
                : query.OrderBy(x => x.Bank != null ? x.Bank.BankName : null),
            "isdefault" => isDescending
                ? query.OrderByDescending(x => x.IsDefault)
                : query.OrderBy(x => x.IsDefault),
            "isactive" => isDescending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),
            "createdat" => isDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
            "updatedat" => isDescending
                ? query.OrderByDescending(x => x.UpdatedAt)
                : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
