using MediatR;
using TLBIOMASS.Application.Features.Receivers.DTOs;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById
{
    public class GetReceiverByIdQuery : IRequest<ReceiverResponseDto>
    {
        public int Id { get; set; }
    }
}
