using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Agencies.DTOs;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;

public class GetAgenciesQuery : IRequest<PagedList<AgencyResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedDate";
    public string? SortDirection { get; set; } = "desc";
}
