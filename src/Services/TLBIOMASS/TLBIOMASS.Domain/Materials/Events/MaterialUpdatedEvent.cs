using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Materials.Events;

public record MaterialUpdatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public MaterialUpdatedEvent(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
