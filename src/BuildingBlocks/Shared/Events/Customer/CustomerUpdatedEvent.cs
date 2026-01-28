using Shared.Interfaces.Event;

namespace Shared.Events.Customer;

public record class CustomerUpdatedEvent : BaseEvent
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}
