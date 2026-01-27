using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.MaterialRegions.Events;

public record MaterialRegionUpdatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;

    public MaterialRegionUpdatedEvent(int id, string regionName)
    {
        Id = id;
        RegionName = regionName;
    }
}
