using MediatR;
using Shared.DTOs.Receiver;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQuery : IRequest<List<ReceiverResponseDto>>
{
    public ReceiverFilterDto Filter { get; set; } = new();
}

