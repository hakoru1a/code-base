using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Landowners;
using Shared.Domain.Enums;
using Contracts.Domain.Enums;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownersPaged;

public class GetLandownersPagedQueryHandler : IRequestHandler<GetLandownersPagedQuery, PagedList<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetLandownersPagedQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<LandownerResponseDto>> Handle(GetLandownersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll(false, x => x.BankAccounts.Where(b => b.OwnerType == OwnerType.Landowner));

        query = ApplyFilter(query, request.Filter);

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<LandownerResponseDto>(
            pagedItems.Adapt<List<LandownerResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Landowner> ApplyFilter(IQueryable<Landowner> query, LandownerPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

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

    private static IQueryable<Landowner> ApplySort(IQueryable<Landowner> query, string? orderBy, string? direction)
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
            "status" => isDescending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),
            "createddate" => isDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" => isDescending
                ? query.OrderByDescending(x => x.LastModifiedDate)
                : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
