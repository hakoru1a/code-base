using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Landowners.Events;

public record LandownerCreatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public LandownerCreatedEvent(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
