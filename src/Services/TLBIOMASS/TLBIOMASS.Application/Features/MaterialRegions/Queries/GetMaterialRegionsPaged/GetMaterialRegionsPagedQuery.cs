using MediatR;
using Shared.DTOs.MaterialRegion;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionsPaged;

public class GetMaterialRegionsPagedQuery : IRequest<PagedList<MaterialRegionResponseDto>>
{
    public MaterialRegionPagedFilterDto Filter { get; set; } = new();
}

