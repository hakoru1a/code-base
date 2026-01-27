using MediatR;
using TLBIOMASS.Application.Features.Agencies.DTOs;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAllAgencies;

public class GetAllAgenciesQuery : IRequest<List<AgencyResponseDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
