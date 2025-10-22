using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Category
{
    public record class CategoryUpdatedEvent : BaseEvent
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

