using MediatR;
using Shared.DTOs.Receiver;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetAllReceivers;

public class GetAllReceiversQuery : IRequest<List<ReceiverResponseDto>>
{
    public ReceiverFilterDto Filter { get; set; } = new();
}
