using MediatR;

namespace Shared.Events.Product
{
    public class ProductCreatedEvent : INotification
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

