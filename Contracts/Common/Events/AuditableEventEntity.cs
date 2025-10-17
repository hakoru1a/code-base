using Contracts.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Common.Events
{
    public abstract class AuditableEventEntity<T> : EventEntity<T>, IAuditable<T>
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public T CreatedBy { get; set; }
        public T? LastModifiedBy { get; set; }
    }


}
