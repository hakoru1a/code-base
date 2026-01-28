using Shared.Interfaces.Event;

namespace Shared.Events.Landowner;

public record class LandownerUpdatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
