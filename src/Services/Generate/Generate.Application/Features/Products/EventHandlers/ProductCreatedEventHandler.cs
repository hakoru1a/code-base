using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Product;

namespace Generate.Application.Features.Products.EventHandlers
{
    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedEventHandler> _logger;

        public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product Created: {ProductId} - {Name} in Category: {CategoryId} at {CreatedDate}",
                notification.ProductId, notification.Name, notification.CategoryId, notification.CreatedDate);

            // Add additional business logic here (e.g., update search index, invalidate cache, etc.)

            return Task.CompletedTask;
        }
    }
}

