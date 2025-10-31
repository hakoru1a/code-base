using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Product
{
    public record class ProductDeletedEvent : BaseEvent
    {
        public long ProductId { get; set; }
    }
}

