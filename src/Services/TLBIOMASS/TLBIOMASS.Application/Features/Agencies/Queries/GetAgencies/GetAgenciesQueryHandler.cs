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

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<AgencyResponseDto>(
            pagedItems.Adapt<List<AgencyResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }

}
