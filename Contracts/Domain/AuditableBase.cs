using Contracts.Domain.Interface;

namespace Contracts.Domain
{
    public abstract class AuditableBase<T> : IAuditable<T>
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public T CreatedBy { get; set; }
        public T? LastModifiedBy { get; set; }
    }
}