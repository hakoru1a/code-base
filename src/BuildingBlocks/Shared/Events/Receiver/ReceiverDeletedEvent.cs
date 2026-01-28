using Shared.Interfaces.Event;

namespace Shared.Events.Receiver;

public record class ReceiverDeletedEvent : BaseEvent
{
    public int ReceiverId { get; set; }
}
