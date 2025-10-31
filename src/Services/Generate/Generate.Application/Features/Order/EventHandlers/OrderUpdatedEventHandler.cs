using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Order;

namespace Generate.Application.Features.Order.EventHandlers
{
    public class OrderUpdatedEventHandler : INotificationHandler<OrderUpdatedEvent>
    {
        private readonly ILogger<OrderUpdatedEventHandler> _logger;

        public OrderUpdatedEventHandler(ILogger<OrderUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order Updated: {OrderId} - Customer: {CustomerName} at {UpdatedDate}",
                notification.OrderId, notification.CustomerName, notification.UpdatedDate);

            // Add additional business logic here (e.g., send update notifications, sync with external systems, etc.)

            return Task.CompletedTask;
        }
    }
}

