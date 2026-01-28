using MediatR;
using Shared.DTOs.WeighingTicketCancel;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancelById;

public class GetWeighingTicketCancelByIdQuery : IRequest<WeighingTicketCancelResponseDto>
{
    public int Id { get; set; }

    public GetWeighingTicketCancelByIdQuery(int id)
    {
        Id = id;
    }
}
