using Contracts.Common.Interface;
using Contracts.Domain;
using Contracts.Domain.Interface;
using Shared.Interfaces.Event;

namespace Contracts.Common.Events
{
    public class EventEntity<T> : EntityBase<T>, IEventEntity<T>, IDateTracking, IUserTracking<T>
    {
        private List<BaseEvent> _events = new();
        public IReadOnlyCollection<BaseEvent> DomainEvents => _events.AsReadOnly();

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public T? CreatedBy { get; set; }
        public T? LastModifiedBy { get; set; }

        public void AddDomainEvent(BaseEvent domainEvent)
        {
            _events.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _events.Clear();
        }

        public void RemoveDomainEvent(BaseEvent domainEvent)
        {
            _events.Remove(domainEvent);
        }
    }
}
