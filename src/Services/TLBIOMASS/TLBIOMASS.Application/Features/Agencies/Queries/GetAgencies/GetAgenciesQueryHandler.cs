using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Shared.SeedWork;
using Mapster;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Specifications;
using System.Linq;

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
        // Start with base query
        var query = _repository.FindAll();

        // Apply filters using Specifications as per Architecture Guide
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new AgencySearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new AgencyIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<AgencyResponseDto>(
            pagedItems.Adapt<List<AgencyResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }

    private IQueryable<Agency> ApplySorting(IQueryable<Agency> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "phone" => isDescending ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
            "email" => isDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "bankaccount" => isDescending ? query.OrderByDescending(x => x.BankAccount) : query.OrderBy(x => x.BankAccount),
            "createddate" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" or "updated" => isDescending ? query.OrderByDescending(x => x.LastModifiedDate) : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
