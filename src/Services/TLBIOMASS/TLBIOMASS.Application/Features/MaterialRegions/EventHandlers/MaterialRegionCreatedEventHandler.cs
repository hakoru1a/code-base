using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.MaterialRegion;

namespace TLBIOMASS.Application.Features.MaterialRegions.EventHandlers;

public class MaterialRegionCreatedEventHandler : INotificationHandler<MaterialRegionCreatedEvent>
{
    private readonly ILogger<MaterialRegionCreatedEventHandler> _logger;

    public MaterialRegionCreatedEventHandler(ILogger<MaterialRegionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialRegionCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: MaterialRegion with Id {Id} and Name {Name} has been created.", 
            notification.Id, notification.RegionName);

        return Task.CompletedTask;
    }
}
