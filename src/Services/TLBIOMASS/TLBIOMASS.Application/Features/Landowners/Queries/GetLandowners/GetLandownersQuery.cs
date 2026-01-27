using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Landowners.DTOs;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQuery : IRequest<PagedList<LandownerResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedDate";
    public string? SortDirection { get; set; } = "desc";
}
