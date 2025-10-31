using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Order
{
    public record class OrderUpdatedEvent : BaseEvent
    {
        public long OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }
}

