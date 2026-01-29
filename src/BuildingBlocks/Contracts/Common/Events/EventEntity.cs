using System.Collections;
using System.Linq;
using Contracts.Common.Interface;
using Contracts.Domain;
using Contracts.Domain.Interface;

namespace Contracts.Common.Events
{
    public class EventEntity<T, TEvent> : EntityBase<T>, IEventEntity<T, TEvent>, IDateTracking, IUserTracking<T>
        where TEvent : class
    {
        private readonly List<TEvent> _events = new();
        public IReadOnlyCollection<TEvent> DomainEvents => _events.AsReadOnly();
        IEnumerable<object> IEventEntity.DomainEvents => DomainEvents.Cast<object>();

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public T? CreatedBy { get; set; }
        public T? LastModifiedBy { get; set; }

        public void AddDomainEvent(TEvent domainEvent)
        {
            _events.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _events.Clear();
        }

        public void RemoveDomainEvent(TEvent domainEvent)
        {
            _events.Remove(domainEvent);
        }
    }
}
