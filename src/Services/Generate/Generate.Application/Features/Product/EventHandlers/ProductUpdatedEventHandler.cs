using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Product;

namespace Generate.Application.Features.Product.EventHandlers
{
    public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedEvent>
    {
        private readonly ILogger<ProductUpdatedEventHandler> _logger;

        public ProductUpdatedEventHandler(ILogger<ProductUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product Updated: {ProductId} - {Name} in Category: {CategoryId} at {UpdatedDate}",
                notification.ProductId, notification.Name, notification.CategoryId, notification.UpdatedDate);

            // Add additional business logic here (e.g., update search index, invalidate cache, etc.)

            return Task.CompletedTask;
        }
    }
}

