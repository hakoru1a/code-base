using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Customers.Events;

namespace TLBIOMASS.Application.Features.Customers.EventHandlers;

public class CustomerDeletedEventHandler : INotificationHandler<CustomerDeletedEvent>
{
    private readonly ILogger<CustomerDeletedEventHandler> _logger;

    public CustomerDeletedEventHandler(ILogger<CustomerDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer Deleted: {CustomerId}", notification.CustomerId);

        return Task.CompletedTask;
    }
}
