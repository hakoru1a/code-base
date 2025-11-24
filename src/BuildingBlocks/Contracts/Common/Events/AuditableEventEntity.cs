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
        public new DateTimeOffset CreatedDate { get; set; }
        public new DateTimeOffset? LastModifiedDate { get; set; }
        public new T? CreatedBy { get; set; }
        public new T? LastModifiedBy { get; set; }
    }


}
