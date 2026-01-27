using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Agencies.Events;

public record AgencyCreatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public AgencyCreatedEvent(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
