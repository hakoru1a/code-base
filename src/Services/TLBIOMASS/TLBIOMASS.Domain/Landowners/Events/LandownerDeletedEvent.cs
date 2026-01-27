using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Landowners.Events;

public record LandownerDeletedEvent : BaseEvent
{
    public int Id { get; set; }

    public LandownerDeletedEvent(int id)
    {
        Id = id;
    }
}
