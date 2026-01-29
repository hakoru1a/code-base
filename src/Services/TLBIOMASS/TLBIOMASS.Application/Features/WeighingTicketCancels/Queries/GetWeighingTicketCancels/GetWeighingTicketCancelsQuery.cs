using MediatR;
using Shared.SeedWork;
using Shared.DTOs.WeighingTicketCancel;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancels;

public class GetWeighingTicketCancelsQuery : IRequest<PagedList<WeighingTicketCancelResponseDto>>
{
    public WeighingTicketCancelPagedFilterDto Filter { get; set; } = new();
}
