using Shared.Interfaces.Event;

namespace Shared.Events.Landowner;

public record class LandownerDeletedEvent : BaseEvent
{
    public int Id { get; set; }
}
