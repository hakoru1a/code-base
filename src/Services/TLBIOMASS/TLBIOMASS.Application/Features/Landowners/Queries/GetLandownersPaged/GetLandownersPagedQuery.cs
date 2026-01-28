using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownersPaged;

public class GetLandownersPagedQuery : IRequest<PagedList<LandownerResponseDto>>
{
    public LandownerPagedFilterDto Filter { get; set; } = new();
}

