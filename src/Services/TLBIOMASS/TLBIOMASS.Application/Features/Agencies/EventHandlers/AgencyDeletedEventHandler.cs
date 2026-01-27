using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Agency;

namespace TLBIOMASS.Application.Features.Agencies.EventHandlers;

public class AgencyDeletedEventHandler : INotificationHandler<AgencyDeletedEvent>
{
    private readonly ILogger<AgencyDeletedEventHandler> _logger;

    public AgencyDeletedEventHandler(ILogger<AgencyDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AgencyDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Agency with Id {Id} has been deleted.", notification.Id);

        return Task.CompletedTask;
    }
}
