using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Application.Features.Agencies.DTOs;
using TLBIOMASS.Domain.Agencies.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Agencies;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAllAgencies;

public class GetAllAgenciesQueryHandler : IRequestHandler<GetAllAgenciesQuery, List<AgencyResponseDto>>
{
    private readonly IAgencyRepository _repository;

    public GetAllAgenciesQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AgencyResponseDto>> Handle(GetAllAgenciesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Search))
        {
            var spec = new AgencySearchSpecification(request.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.IsActive.HasValue)
        {
            var spec = new AgencyIsActiveSpecification(request.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting (reuse logic or simple default)
        // Since we don't have SortBy in GetAllAgenciesQuery, defaults to CreatedDate desc
        query = query.OrderByDescending(x => x.CreatedDate);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<AgencyResponseDto>>();
    }
}
