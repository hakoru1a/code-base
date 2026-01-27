using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Landowners.Events;

namespace TLBIOMASS.Application.Features.Landowners.EventHandlers;

public class LandownerUpdatedEventHandler : INotificationHandler<LandownerUpdatedEvent>
{
    private readonly ILogger<LandownerUpdatedEventHandler> _logger;

    public LandownerUpdatedEventHandler(ILogger<LandownerUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LandownerUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Landowner with Id {Id} has been updated. New name: {Name}", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
