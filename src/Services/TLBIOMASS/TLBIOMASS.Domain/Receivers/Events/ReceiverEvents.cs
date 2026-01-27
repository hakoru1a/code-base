using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Receivers.Events;

public record ReceiverCreatedEvent : BaseEvent
{
    public int ReceiverId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record ReceiverUpdatedEvent : BaseEvent
{
    public int ReceiverId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record ReceiverDeletedEvent : BaseEvent
{
    public int ReceiverId { get; set; }
}
