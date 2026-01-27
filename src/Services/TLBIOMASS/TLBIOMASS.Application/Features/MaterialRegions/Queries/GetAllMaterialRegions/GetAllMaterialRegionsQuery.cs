using MediatR;
using Shared.DTOs.MaterialRegion;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetAllMaterialRegions;

public class GetAllMaterialRegionsQuery : IRequest<List<MaterialRegionResponseDto>>
{
    public MaterialRegionFilterDto Filter { get; set; } = new();
}
