using Contracts.Common.Events;

namespace Contracts.Domain.Event.Product
{
    public record ProductDeletedEvent(
        long ProductId,
        string Name,
        string SKU,
        DateTimeOffset DeletedAt
    ) : BaseEvent;
}
