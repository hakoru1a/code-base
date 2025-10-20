using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Order;

namespace Generate.Application.Features.Order.EventHandlers
{
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order Created: {OrderId} - Customer: {CustomerName} with {TotalItems} items at {CreatedDate}",
                notification.OrderId, notification.CustomerName, notification.TotalItems, notification.CreatedDate);

            // Add additional business logic here (e.g., send order confirmation email, update inventory, etc.)

            return Task.CompletedTask;
        }
    }
}

