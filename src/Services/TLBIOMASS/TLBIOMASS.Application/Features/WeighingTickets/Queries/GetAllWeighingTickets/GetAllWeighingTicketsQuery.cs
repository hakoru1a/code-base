using MediatR;
using Shared.DTOs.WeighingTicket;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetAllWeighingTickets;

public class GetAllWeighingTicketsQuery : IRequest<List<WeighingTicketResponseDto>>
{
    public WeighingTicketFilterDto Filter { get; set; } = new();
}
