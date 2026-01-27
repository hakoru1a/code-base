using MediatR;
using Shared.DTOs.Receiver;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById
{
    public class GetReceiverByIdQuery : IRequest<ReceiverResponseDto>
    {
        public int Id { get; set; }
    }
}
