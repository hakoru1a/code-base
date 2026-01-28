using Shared.Interfaces.Event;

namespace Shared.Events.MaterialRegion;

public record class MaterialRegionDeletedEvent : BaseEvent
{
    public int Id { get; set; }
}
