using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Product
{
    public record class ProductCreatedEvent : BaseEvent
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

