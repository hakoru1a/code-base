using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Order
{
    public record class OrderDeletedEvent : BaseEvent
    {
        public long OrderId { get; set; }
    }
}

