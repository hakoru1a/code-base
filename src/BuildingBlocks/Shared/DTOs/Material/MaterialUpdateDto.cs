
using Contracts.Domain.Enums;
namespace Shared.DTOs.Material;

public class MaterialUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
}
