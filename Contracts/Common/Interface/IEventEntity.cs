using Contracts.Common.Events;
using Contracts.Domain.Interface;
using Shared.Interfaces.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Common.Interface
{
    public interface IEventEntity
    {
        void AddDomainEvent(BaseEvent domainEvent);
        void RemoveDomainEvent(BaseEvent domainEvent);
        void ClearDomainEvents();
        IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    }

    public interface IEventEntity<T> : IEnityBase<T>, IEventEntity
    {

    }
}
