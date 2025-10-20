using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Order;

namespace Generate.Application.Features.Order.EventHandlers
{
    public class OrderDeletedEventHandler : INotificationHandler<OrderDeletedEvent>
    {
        private readonly ILogger<OrderDeletedEventHandler> _logger;

        public OrderDeletedEventHandler(ILogger<OrderDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order Deleted: {OrderId} at {DeletedDate}",
                notification.OrderId, notification.DeletedDate);

            // Add additional business logic here (e.g., restore inventory, send cancellation email, etc.)

            return Task.CompletedTask;
        }
    }
}

