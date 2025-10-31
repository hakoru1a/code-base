using MediatR;
using Shared.Interfaces.Event;

namespace Shared.Events.Category
{
    public record class  CategoryCreatedEvent : BaseEvent
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

