using MediatR;

namespace Shared.Events.Order
{
    public class OrderDeletedEvent : INotification
    {
        public long OrderId { get; set; }
        public DateTime DeletedDate { get; set; }
    }
}

