using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQuery : IRequest<PagedList<LandownerResponseDto>>
{
    public LandownerFilterDto Filter { get; set; } = new();
}
