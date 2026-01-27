using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.MaterialRegions.Events;

public record MaterialRegionCreatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;

    public MaterialRegionCreatedEvent(int id, string regionName)
    {
        Id = id;
        RegionName = regionName;
    }
}
