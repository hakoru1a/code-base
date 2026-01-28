using Shared.Interfaces.Event;

namespace Shared.Events.Customer;

public record class CustomerCreatedEvent : BaseEvent
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}
