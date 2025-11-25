using Contracts.Domain;

namespace Generate.Domain.Entities;

public class OrderItem : EntityAuditBase<long>
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
