using Contracts.Common.Events;

namespace Contracts.Domain.Event.Product
{
    public record StockChangedEvent(
        long ProductId,
        string ProductName,
        string SKU,
        int OldStock,
        int NewStock,
        int StockDifference,
        DateTimeOffset ChangedAt
    ) : BaseEvent;
}
