using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Landowners.Events;

namespace TLBIOMASS.Application.Features.Landowners.EventHandlers;

public class LandownerDeletedEventHandler : INotificationHandler<LandownerDeletedEvent>
{
    private readonly ILogger<LandownerDeletedEventHandler> _logger;

    public LandownerDeletedEventHandler(ILogger<LandownerDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LandownerDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Landowner with Id {Id} has been deleted.", notification.Id);

        return Task.CompletedTask;
    }
}
