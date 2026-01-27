using Shared.Interfaces.Event;

namespace Shared.Events.Material;

public record class MaterialCreatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
