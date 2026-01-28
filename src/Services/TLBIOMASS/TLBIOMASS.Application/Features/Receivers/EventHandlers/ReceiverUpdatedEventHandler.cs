using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Receiver;

namespace TLBIOMASS.Application.Features.Receivers.EventHandlers;

public class ReceiverUpdatedEventHandler : INotificationHandler<ReceiverUpdatedEvent>
{
    private readonly ILogger<ReceiverUpdatedEventHandler> _logger;

    public ReceiverUpdatedEventHandler(ILogger<ReceiverUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ReceiverUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Receiver with Id {ReceiverId} and Name {Name} has been updated.", 
            notification.ReceiverId, notification.Name);

        return Task.CompletedTask;
    }
}
