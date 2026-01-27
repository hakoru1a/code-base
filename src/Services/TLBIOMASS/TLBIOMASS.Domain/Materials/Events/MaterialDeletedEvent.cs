using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Materials.Events;

public record MaterialDeletedEvent : BaseEvent
{
    public int Id { get; set; }

    public MaterialDeletedEvent(int id)
    {
        Id = id;
    }
}
