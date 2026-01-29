using MediatR;
using Shared.SeedWork;
using Shared.DTOs.WeighingTicket;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTicketsPaged;

public class GetWeighingTicketsPagedQuery : IRequest<PagedList<WeighingTicketResponseDto>>
{
    public WeighingTicketPagedFilterDto Filter { get; set; } = new();
}

