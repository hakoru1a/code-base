using Contracts.Common.Events;

namespace Contracts.Domain.Event.Product
{
    public record ProductUpdatedEvent(
        long ProductId,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        string SKU,
        DateTimeOffset UpdatedAt
    ) : BaseEvent;
}
