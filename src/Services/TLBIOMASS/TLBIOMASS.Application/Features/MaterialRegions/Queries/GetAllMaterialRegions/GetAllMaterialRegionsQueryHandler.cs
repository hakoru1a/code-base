using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Shared.DTOs.MaterialRegion;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.MaterialRegions.Specifications;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetAllMaterialRegions;

public class GetAllMaterialRegionsQueryHandler : IRequestHandler<GetAllMaterialRegionsQuery, List<MaterialRegionResponseDto>>
{
    private readonly IMaterialRegionRepository _repository;

    public GetAllMaterialRegionsQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaterialRegionResponseDto>> Handle(GetAllMaterialRegionsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Filters
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var spec = new MaterialRegionSearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        // Removed IsActive check based on user feedback

        // 2. Fetch and Adapt (No sorting)
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<MaterialRegionResponseDto>>();
    }
}
