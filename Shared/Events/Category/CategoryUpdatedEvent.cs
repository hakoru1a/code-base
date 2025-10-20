using MediatR;

namespace Shared.Events.Category
{
    public class CategoryUpdatedEvent : INotification
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }
}

