using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Product;

namespace Generate.Application.Features.Products.EventHandlers
{
    public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedEvent>
    {
        private readonly ILogger<ProductDeletedEventHandler> _logger;

        public ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product Deleted: {ProductId} at {DeletedDate}",
                notification.ProductId, notification.EventId);

            // Add additional business logic here (e.g., update search index, invalidate cache, etc.)

            return Task.CompletedTask;
        }
    }
}

