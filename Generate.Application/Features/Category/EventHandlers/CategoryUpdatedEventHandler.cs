using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Category;

namespace Generate.Application.Features.Category.EventHandlers
{
    public class CategoryUpdatedEventHandler : INotificationHandler<CategoryUpdatedEvent>
    {
        private readonly ILogger<CategoryUpdatedEventHandler> _logger;

        public CategoryUpdatedEventHandler(ILogger<CategoryUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category Updated: {CategoryId} - {Name} at {UpdatedDate}",
                notification.CategoryId, notification.Name, notification.UpdatedDate);

            // Add additional business logic here (e.g., send notifications, update cache, etc.)

            return Task.CompletedTask;
        }
    }
}

