using Shared.Interfaces.Event;

namespace Shared.Events.Agency;

public record class AgencyDeletedEvent : BaseEvent
{
    public int Id { get; set; }
}
