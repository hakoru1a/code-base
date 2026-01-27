using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Agencies.Events;

public record AgencyDeletedEvent : BaseEvent
{
    public int Id { get; set; }

    public AgencyDeletedEvent(int id)
    {
        Id = id;
    }
}
