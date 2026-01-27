using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.MaterialRegions.Events;

public record MaterialRegionDeletedEvent : BaseEvent
{
    public int Id { get; set; }

    public MaterialRegionDeletedEvent(int id)
    {
        Id = id;
    }
}
