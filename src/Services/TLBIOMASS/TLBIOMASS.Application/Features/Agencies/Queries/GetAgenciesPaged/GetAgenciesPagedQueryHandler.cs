using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Agencies;
using Shared.Domain.Enums;
using Contracts.Domain.Enums;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgenciesPaged;

public class GetAgenciesPagedQueryHandler : IRequestHandler<GetAgenciesPagedQuery, PagedList<AgencyResponseDto>>
{
    private readonly IAgencyRepository _repository;

    public GetAgenciesPagedQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<AgencyResponseDto>> Handle(GetAgenciesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll(false, x => x.BankAccounts.Where(b => b.OwnerType == OwnerType.Agency));
        query = ApplyFilter(query, request.Filter);
        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<AgencyResponseDto>(
            pagedItems.Adapt<List<AgencyResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Agency> ApplyFilter(IQueryable<Agency> query, AgencyPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(x => x.Name.Contains(filter.Name));

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerms))
        {
            var search = filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.BankAccounts.Any(ba => ba.AccountNumber.Contains(search) || ba.BankName.ToLower().Contains(search))) ||
                               (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(search)));
        }

        return query;
    }

    private static IQueryable<Agency> ApplySort(IQueryable<Agency> query, string? orderBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return query.OrderBy(x => x.Id); // Default sort
        }

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "bankname" => isDescending
                ? query.OrderByDescending(x => x.BankAccounts.Where(ba => ba.IsDefault).Select(ba => ba.BankName).FirstOrDefault())
                : query.OrderBy(x => x.BankAccounts.Where(ba => ba.IsDefault).Select(ba => ba.BankName).FirstOrDefault()),

            "status" => isDescending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            "createdate" => isDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),

            "updatedate" => isDescending
                ? query.OrderByDescending(x => x.LastModifiedDate)
                : query.OrderBy(x => x.LastModifiedDate),

            _ => query.OrderBy(x => x.Id) // Fallback to default
        };
    }
}
