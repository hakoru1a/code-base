using MediatR;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.DeleteWeighingTicketCancel;

public class DeleteWeighingTicketCancelCommand : IRequest<bool>
{
    public int Id { get; set; } 

    public DeleteWeighingTicketCancelCommand(int id)
    {
        Id = id;
    }
}
