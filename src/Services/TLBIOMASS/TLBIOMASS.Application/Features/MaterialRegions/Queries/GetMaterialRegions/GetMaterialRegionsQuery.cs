using MediatR;
using Shared.DTOs.MaterialRegion;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;

public class GetMaterialRegionsQuery : IRequest<PagedList<MaterialRegionResponseDto>>
{
    public MaterialRegionPagedFilterDto Filter { get; set; } = new();
}
