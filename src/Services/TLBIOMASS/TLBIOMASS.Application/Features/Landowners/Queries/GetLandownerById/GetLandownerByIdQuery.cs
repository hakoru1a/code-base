using MediatR;
using TLBIOMASS.Application.Features.Landowners.DTOs;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById
{
    public class GetLandownerByIdQuery : IRequest<LandownerResponseDto>
    {
        public int Id { get; set; }
    }
}
