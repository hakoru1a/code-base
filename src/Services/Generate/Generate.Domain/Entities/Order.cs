using Contracts.Domain;

namespace Generate.Domain.Entities;

public class Order : EntityAuditBase<long>
{
    public string CustomerName { get; set; } = string.Empty;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
