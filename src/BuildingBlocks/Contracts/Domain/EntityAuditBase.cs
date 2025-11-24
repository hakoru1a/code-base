using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Domain.Interface;

namespace Contracts.Domain
{
    public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable<T>
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public T? CreatedBy { get; set; }
        public T? LastModifiedBy { get; set; }
    }
}
