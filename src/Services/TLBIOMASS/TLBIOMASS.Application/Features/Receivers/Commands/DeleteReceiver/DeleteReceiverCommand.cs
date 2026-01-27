using MediatR;

namespace TLBIOMASS.Application.Features.Receivers.Commands.DeleteReceiver;

public class DeleteReceiverCommand : IRequest<bool>
{
    public int Id { get; set; }

    public DeleteReceiverCommand(int id)
    {
        Id = id;
    }
}
