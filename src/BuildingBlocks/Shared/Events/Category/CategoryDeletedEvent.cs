using Shared.Interfaces.Event;

namespace Shared.Events.Category
{
    public record class CategoryDeletedEvent : BaseEvent
    {
        public long CategoryId { get; set; }
    }
}

