using System.Collections;
using Contracts.Domain.Interface;

namespace Contracts.Common.Interface
{
    /// <summary>
    /// Marker for entities that have domain events. Used for ChangeTracker.Entries&lt;IEventEntity&gt;().
    /// </summary>
    public interface IEventEntity
    {
        IEnumerable<object> DomainEvents { get; }
        void ClearDomainEvents();
    }

    /// <summary>
    /// Entity with domain events. T = entity id type, TEvent = domain event type.
    /// </summary>
    public interface IEventEntity<T, TEvent> : IEnityBase<T>, IEventEntity
    {
        void AddDomainEvent(TEvent domainEvent);
        void RemoveDomainEvent(TEvent domainEvent);
        new void ClearDomainEvents();
        new IReadOnlyCollection<TEvent> DomainEvents { get; }
    }
}
