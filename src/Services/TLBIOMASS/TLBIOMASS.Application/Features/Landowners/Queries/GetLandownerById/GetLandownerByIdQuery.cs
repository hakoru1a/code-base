using MediatR;
using Shared.DTOs.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById
{
    public class GetLandownerByIdQuery : IRequest<LandownerResponseDto>
    {
        public int Id { get; set; }
    }
}
