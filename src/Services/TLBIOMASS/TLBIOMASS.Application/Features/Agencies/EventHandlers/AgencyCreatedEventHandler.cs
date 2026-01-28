using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Agency;

namespace TLBIOMASS.Application.Features.Agencies.EventHandlers;

public class AgencyCreatedEventHandler : INotificationHandler<AgencyCreatedEvent>
{
    private readonly ILogger<AgencyCreatedEventHandler> _logger;

    public AgencyCreatedEventHandler(ILogger<AgencyCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AgencyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Agency with Id {Id} and Name {Name} has been created.", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
