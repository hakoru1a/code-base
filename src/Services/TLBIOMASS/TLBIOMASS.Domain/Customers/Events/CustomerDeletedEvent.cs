using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Customers.Events;

public record CustomerDeletedEvent : BaseEvent
{
    public int CustomerId { get; set; }
}
