using Shared.Interfaces.Event;

namespace Shared.Events.Customer;

public record class CustomerDeletedEvent : BaseEvent
{
    public int CustomerId { get; set; }
}
