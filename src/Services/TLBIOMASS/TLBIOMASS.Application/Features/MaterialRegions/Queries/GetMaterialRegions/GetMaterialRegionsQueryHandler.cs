using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Shared.DTOs.MaterialRegion;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;

public class GetMaterialRegionsQueryHandler : IRequestHandler<GetMaterialRegionsQuery, List<MaterialRegionResponseDto>>
{
    private readonly IMaterialRegionRepository _repository;

    public GetMaterialRegionsQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaterialRegionResponseDto>> Handle(GetMaterialRegionsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Filters
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(x => x.Detail.RegionName.ToLower().Contains(search) || 
                               (x.Detail.Address != null && x.Detail.Address.ToLower().Contains(search)) ||
                               (x.Detail.CertificateId != null && x.Detail.CertificateId.ToLower().Contains(search)));
        }

        // 2. Fetch with Includes and Adapt
        var entities = await query
            .Include(x => x.Owner)
            .Include(x => x.RegionMaterials)
            .ThenInclude(x => x.Material)
            .ToListAsync(cancellationToken);
            
        return entities.Adapt<List<MaterialRegionResponseDto>>();
    }
}

