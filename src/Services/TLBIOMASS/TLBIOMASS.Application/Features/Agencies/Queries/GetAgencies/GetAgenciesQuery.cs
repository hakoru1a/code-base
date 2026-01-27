using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;

public class GetAgenciesQuery : IRequest<PagedList<AgencyResponseDto>>
{
    public AgencyFilterDto Filter { get; set; } = new();
}
