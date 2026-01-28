using MediatR;
using Shared.SeedWork;
using Shared.DTOs.WeighingTicket;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTickets;

public class GetWeighingTicketsQuery : IRequest<PagedList<WeighingTicketResponseDto>>
{
    public WeighingTicketPagedFilterDto Filter { get; set; } = new();
}
