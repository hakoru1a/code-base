using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Landowner;

namespace TLBIOMASS.Application.Features.Landowners.EventHandlers;

public class LandownerCreatedEventHandler : INotificationHandler<LandownerCreatedEvent>
{
    private readonly ILogger<LandownerCreatedEventHandler> _logger;

    public LandownerCreatedEventHandler(ILogger<LandownerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LandownerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Landowner with Id {Id} and Name {Name} has been created.", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
