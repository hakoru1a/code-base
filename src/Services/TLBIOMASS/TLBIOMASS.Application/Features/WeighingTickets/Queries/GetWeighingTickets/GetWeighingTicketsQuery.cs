using MediatR;
using Shared.DTOs.WeighingTicket;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTickets;

public class GetWeighingTicketsQuery : IRequest<List<WeighingTicketResponseDto>>
{
    public WeighingTicketFilterDto Filter { get; set; } = new();
}

