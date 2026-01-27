using Shared.Interfaces.Event;

namespace Shared.Events.Material;

public record class MaterialDeletedEvent : BaseEvent
{
    public int Id { get; set; }
}
