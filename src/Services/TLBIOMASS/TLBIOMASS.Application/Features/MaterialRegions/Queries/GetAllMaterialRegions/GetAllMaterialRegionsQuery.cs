using MediatR;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetAllMaterialRegions;

public class GetAllMaterialRegionsQuery : IRequest<List<MaterialRegionResponseDto>>
{
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
}
