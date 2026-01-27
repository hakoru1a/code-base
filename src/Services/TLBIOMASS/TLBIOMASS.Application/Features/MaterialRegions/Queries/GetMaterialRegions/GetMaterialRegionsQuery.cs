using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;

public class GetMaterialRegionsQuery : IRequest<PagedList<MaterialRegionResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
    public string? SortBy { get; set; } = "CreatedDate";
    public string? SortDirection { get; set; } = "desc";
}
