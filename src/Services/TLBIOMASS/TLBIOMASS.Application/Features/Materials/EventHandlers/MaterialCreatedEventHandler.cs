using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Material;

namespace TLBIOMASS.Application.Features.Materials.EventHandlers;

public class MaterialCreatedEventHandler : INotificationHandler<MaterialCreatedEvent>
{
    private readonly ILogger<MaterialCreatedEventHandler> _logger;

    public MaterialCreatedEventHandler(ILogger<MaterialCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaterialCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Material with Id {Id} and Name {Name} has been created.", 
            notification.Id, notification.Name);

        return Task.CompletedTask;
    }
}
