using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Customers.Events;

namespace TLBIOMASS.Application.Features.Customers.EventHandlers;

public class CustomerUpdatedEventHandler : INotificationHandler<CustomerUpdatedEvent>
{
    private readonly ILogger<CustomerUpdatedEventHandler> _logger;

    public CustomerUpdatedEventHandler(ILogger<CustomerUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer Updated: {CustomerId} - {Name}", 
            notification.CustomerId, notification.TenKhachHang);

        return Task.CompletedTask;
    }
}
