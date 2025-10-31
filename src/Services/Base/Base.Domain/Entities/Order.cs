using Contracts.Domain;

namespace Base.Domain.Entities;
public class OrderBase : EntityAuditBase<long>
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
