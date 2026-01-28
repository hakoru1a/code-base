using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Customers.Events;

public record CustomerCreatedEvent : BaseEvent
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}
