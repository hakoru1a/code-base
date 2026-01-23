using MediatR;
using Microsoft.Extensions.Logging;
using TLBIOMASS.Domain.Customers.Events;

namespace TLBIOMASS.Application.Features.Customers.EventHandlers;

public class CustomerCreatedEventHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer Created: {CustomerId} - {Name}", 
            notification.CustomerId, notification.TenKhachHang);

        return Task.CompletedTask;
    }
}
