using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Agencies.Specifications;
using System.Collections.Generic;
using System.Linq;

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

        // 1. Apply Search Filter
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var spec = new AgencySearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new AgencyIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // 2. Fetch and Adapt (Strictly no sorting as per user request)
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<AgencyResponseDto>>();
    }
}
