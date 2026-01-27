using MediatR;
using Shared.DTOs.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetAllLandowners;

public class GetAllLandownersQuery : IRequest<List<LandownerResponseDto>>
{
    public LandownerFilterDto Filter { get; set; } = new();
}
