using MediatR;

namespace Shared.Events.Order
{
    public class OrderUpdatedEvent : INotification
    {
        public long OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }
}

