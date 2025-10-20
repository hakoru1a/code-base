using Contracts.Domain;

namespace Generate.Domain.Entities;

public class Category  : EntityAuditBase<long>
{
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
