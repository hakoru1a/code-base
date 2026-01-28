using Shared.Interfaces.Event;

namespace Shared.Events.Agency;

public record class AgencyCreatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
