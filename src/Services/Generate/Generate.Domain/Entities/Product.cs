using Contracts.Domain;

namespace Generate.Domain.Entities;

public class Product : EntityAuditBase<long>
{
    public string Name { get; set; } = string.Empty;
    public long? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ProductDetail? ProductDetail { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
