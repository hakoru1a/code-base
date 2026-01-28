using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgenciesPaged;

public class GetAgenciesPagedQuery : IRequest<PagedList<AgencyResponseDto>>
{
    public AgencyPagedFilterDto Filter { get; set; } = new();
}

