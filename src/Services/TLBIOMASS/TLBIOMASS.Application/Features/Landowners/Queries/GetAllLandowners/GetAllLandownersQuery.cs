using MediatR;
using TLBIOMASS.Application.Features.Landowners.DTOs;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetAllLandowners;

public class GetAllLandownersQuery : IRequest<List<LandownerResponseDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
