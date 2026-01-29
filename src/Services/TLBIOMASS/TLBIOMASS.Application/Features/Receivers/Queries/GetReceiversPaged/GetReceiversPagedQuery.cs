using MediatR;
using Shared.DTOs.Receiver;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiversPaged;

public class GetReceiversPagedQuery : IRequest<PagedList<ReceiverResponseDto>>
{
    public ReceiverPagedFilterDto Filter { get; set; } = new();
}

