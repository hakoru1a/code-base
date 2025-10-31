using Contracts.Domain;

namespace Generate.Domain.Entities;

public class ProductDetail : EntityAuditBase<long>
{
    public long ProductId { get; set; }
    public string? Description { get; set; }
    public virtual Product Product { get; set; } = null!;
}
