using MediatR;
using Shared.DTOs.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;

public class GetAgenciesQuery : IRequest<List<AgencyResponseDto>>
{
    public AgencyFilterDto Filter { get; set; } = new();
}

