using Shared.Interfaces.Event;

namespace Shared.Events.Receiver;

public record class ReceiverCreatedEvent : BaseEvent
{
    public int ReceiverId { get; set; }
    public string Name { get; set; } = string.Empty;
}
