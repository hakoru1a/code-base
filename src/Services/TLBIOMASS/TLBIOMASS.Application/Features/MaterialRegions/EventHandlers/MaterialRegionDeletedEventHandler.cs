using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.MaterialRegions.Events;

namespace TLBIOMASS.Application.Features.MaterialRegions.EventHandlers;

public class MaterialRegionDeletedEventHandler : INotificationHandler<MaterialRegionDeletedEvent>
{
    private readonly ILogger<MaterialRegionDeletedEventHandler> _logger;

    public MaterialRegionDeletedEventHandler(ILogger<MaterialRegionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialRegionDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: MaterialRegion with Id {Id} has been deleted.", notification.Id);

        return Task.CompletedTask;
    }
}
