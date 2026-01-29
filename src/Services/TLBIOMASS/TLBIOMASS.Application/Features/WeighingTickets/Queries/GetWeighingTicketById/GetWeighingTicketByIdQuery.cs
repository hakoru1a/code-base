using MediatR;
using Shared.DTOs.WeighingTicket;

namespace TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTicketById;

public class GetWeighingTicketByIdQuery : IRequest<WeighingTicketResponseDto>
{
    // Note: Assuming 'Id' is the unique identifier.
    public int Id { get; set; }

    public GetWeighingTicketByIdQuery(int id)
    {
        Id = id;
    }
}
