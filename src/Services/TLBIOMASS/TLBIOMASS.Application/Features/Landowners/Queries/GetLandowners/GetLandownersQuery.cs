using MediatR;
using Shared.DTOs.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQuery : IRequest<List<LandownerResponseDto>>
{
    public LandownerFilterDto Filter { get; set; } = new();
}

