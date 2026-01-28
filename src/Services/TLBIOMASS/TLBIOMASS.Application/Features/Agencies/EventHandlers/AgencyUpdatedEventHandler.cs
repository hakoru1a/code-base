using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Agency;

namespace TLBIOMASS.Application.Features.Agencies.EventHandlers;

public class AgencyUpdatedEventHandler : INotificationHandler<AgencyUpdatedEvent>
{
    private readonly ILogger<AgencyUpdatedEventHandler> _logger;

    public AgencyUpdatedEventHandler(ILogger<AgencyUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AgencyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Agency with Id {Id} has been updated. New name: {Name}", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
