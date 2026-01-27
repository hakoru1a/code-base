using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Landowners.Events;

public record LandownerUpdatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public LandownerUpdatedEvent(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
