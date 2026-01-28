using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Events.Receiver;

namespace TLBIOMASS.Application.Features.Receivers.EventHandlers;

public class ReceiverCreatedEventHandler : INotificationHandler<ReceiverCreatedEvent>
{
    private readonly ILogger<ReceiverCreatedEventHandler> _logger;

    public ReceiverCreatedEventHandler(ILogger<ReceiverCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ReceiverCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Receiver with Id {ReceiverId} and Name {Name} has been created.", 
            notification.ReceiverId, notification.Name);

        return Task.CompletedTask;
    }
}
