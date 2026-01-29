using MediatR;
using Shared.DTOs.MaterialRegion;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;

public class GetMaterialRegionsQuery : IRequest<List<MaterialRegionResponseDto>>
{
    public MaterialRegionFilterDto Filter { get; set; } = new();
}

