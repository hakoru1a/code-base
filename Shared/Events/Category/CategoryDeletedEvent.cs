using MediatR;

namespace Shared.Events.Category
{
    public class CategoryDeletedEvent : INotification
    {
        public long CategoryId { get; set; }
        public DateTime DeletedDate { get; set; }
    }
}

