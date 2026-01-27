using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Materials.DTOs;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;

public class GetMaterialsQuery : IRequest<PagedList<MaterialResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
