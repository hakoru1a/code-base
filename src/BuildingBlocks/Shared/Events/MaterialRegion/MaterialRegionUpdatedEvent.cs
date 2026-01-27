using Shared.Interfaces.Event;

namespace Shared.Events.MaterialRegion;

public record class MaterialRegionUpdatedEvent : BaseEvent
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;
}
