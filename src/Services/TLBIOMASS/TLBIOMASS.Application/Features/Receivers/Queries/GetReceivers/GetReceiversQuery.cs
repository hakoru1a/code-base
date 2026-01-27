using MediatR;
using Shared.DTOs.Receiver;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQuery : IRequest<PagedList<ReceiverResponseDto>>
{
    public ReceiverPagedFilterDto Filter { get; set; } = new();
}
