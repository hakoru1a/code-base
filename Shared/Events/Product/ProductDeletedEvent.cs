using MediatR;

namespace Shared.Events.Product
{
    public class ProductDeletedEvent : INotification
    {
        public long ProductId { get; set; }
        public DateTime DeletedDate { get; set; }
    }
}

