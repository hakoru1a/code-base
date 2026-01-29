using System.Linq;
using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Mapster;
using TLBIOMASS.Domain.Receivers;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiversPaged;

public class GetReceiversPagedQueryHandler : IRequestHandler<GetReceiversPagedQuery, PagedList<ReceiverResponseDto>>
{
    private readonly IReceiverRepository _repository;

    public GetReceiversPagedQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<ReceiverResponseDto>> Handle(GetReceiversPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll(false, x => x.BankAccounts.Where(b => b.OwnerType == OwnerType.Receiver));

        query = ApplyFilter(query, request.Filter);
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

        if (!string.IsNullOrWhiteSpace(filter.SearchTerms))
        {
            var search = filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.BankAccounts.Any(ba => ba.AccountNumber.Contains(search) || ba.BankName.ToLower().Contains(search))) ||
                               (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(search)));
        }

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
                ? query.OrderByDescending(x => x.BankAccounts.Where(ba => ba.IsDefault).Select(ba => ba.BankName).FirstOrDefault())
                : query.OrderBy(x => x.BankAccounts.Where(ba => ba.IsDefault).Select(ba => ba.BankName).FirstOrDefault()),
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

