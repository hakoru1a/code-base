using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Agencies;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;

public class GetAgenciesQueryHandler : IRequestHandler<GetAgenciesQuery, PagedList<AgencyResponseDto>>
{
    private readonly IAgencyRepository _repository;

    public GetAgenciesQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<AgencyResponseDto>> Handle(GetAgenciesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();
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

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

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
                ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankName : null)
                : query.OrderBy(x => x.Bank != null ? x.Bank.BankName : null),

            "isactive" => isDescending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),

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
