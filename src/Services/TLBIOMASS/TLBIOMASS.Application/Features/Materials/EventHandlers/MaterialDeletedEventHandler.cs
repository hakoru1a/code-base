using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Material;

namespace TLBIOMASS.Application.Features.Materials.EventHandlers;

public class MaterialDeletedEventHandler : INotificationHandler<MaterialDeletedEvent>
{
    private readonly ILogger<MaterialDeletedEventHandler> _logger;

    public MaterialDeletedEventHandler(ILogger<MaterialDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Material with Id {Id} has been deleted.", notification.Id);

        return Task.CompletedTask;
    }
}
