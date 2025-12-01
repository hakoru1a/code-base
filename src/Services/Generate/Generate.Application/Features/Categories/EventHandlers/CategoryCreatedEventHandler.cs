using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Category;

namespace Generate.Application.Features.Categories.EventHandlers
{
    public class CategoryCreatedEventHandler : INotificationHandler<CategoryCreatedEvent>
    {
        private readonly ILogger<CategoryCreatedEventHandler> _logger;

        public CategoryCreatedEventHandler(ILogger<CategoryCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category Created: {CategoryId} - {Name} at {CreatedDate}",
                notification.CategoryId, notification.Name, notification.CreatedDate);

            // Add additional business logic here (e.g., send notifications, update cache, etc.)

            return Task.CompletedTask;
        }
    }
}

