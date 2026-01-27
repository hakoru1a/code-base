using MediatR;
using Shared.DTOs.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAllAgencies;

public class GetAllAgenciesQuery : IRequest<List<AgencyResponseDto>>
{
    public AgencyFilterDto Filter { get; set; } = new();
}
