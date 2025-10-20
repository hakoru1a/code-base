using MediatR;

namespace Shared.Events.Product
{
    public class ProductUpdatedEvent : INotification
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}

