using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Receivers.Events;

namespace TLBIOMASS.Application.Features.Receivers.EventHandlers;

public class ReceiverDeletedEventHandler : INotificationHandler<ReceiverDeletedEvent>
{
    private readonly ILogger<ReceiverDeletedEventHandler> _logger;

    public ReceiverDeletedEventHandler(ILogger<ReceiverDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ReceiverDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Receiver with Id {ReceiverId} has been deleted.", notification.ReceiverId);

        return Task.CompletedTask;
    }
}
