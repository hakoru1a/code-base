using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Materials.Events;

namespace TLBIOMASS.Application.Features.Materials.EventHandlers;

public class MaterialUpdatedEventHandler : INotificationHandler<MaterialUpdatedEvent>
{
    private readonly ILogger<MaterialUpdatedEventHandler> _logger;

    public MaterialUpdatedEventHandler(ILogger<MaterialUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Material with Id {Id} has been updated. New name: {Name}", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
