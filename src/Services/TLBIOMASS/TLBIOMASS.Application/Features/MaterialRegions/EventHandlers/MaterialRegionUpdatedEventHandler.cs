using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.MaterialRegion;

namespace TLBIOMASS.Application.Features.MaterialRegions.EventHandlers;

public class MaterialRegionUpdatedEventHandler : INotificationHandler<MaterialRegionUpdatedEvent>
{
    private readonly ILogger<MaterialRegionUpdatedEventHandler> _logger;

    public MaterialRegionUpdatedEventHandler(ILogger<MaterialRegionUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialRegionUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: MaterialRegion with Id {Id} has been updated. New name: {Name}", 
            notification.Id, notification.RegionName);

        return Task.CompletedTask;
    }
}
