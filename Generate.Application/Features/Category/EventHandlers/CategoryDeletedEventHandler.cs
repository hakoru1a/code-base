using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Category;

namespace Generate.Application.Features.Category.EventHandlers
{
    public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
    {
        private readonly ILogger<CategoryDeletedEventHandler> _logger;

        public CategoryDeletedEventHandler(ILogger<CategoryDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category Deleted: {CategoryId} at {DeletedDate}",
                notification.CategoryId, notification.DeletedDate);

            // Add additional business logic here (e.g., send notifications, update cache, etc.)

            return Task.CompletedTask;
        }
    }
}

