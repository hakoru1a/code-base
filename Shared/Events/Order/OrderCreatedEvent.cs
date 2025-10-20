using MediatR;

namespace Shared.Events.Order
{
    public class OrderCreatedEvent : INotification
    {
        public long OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int TotalItems { get; set; }
    }
}

