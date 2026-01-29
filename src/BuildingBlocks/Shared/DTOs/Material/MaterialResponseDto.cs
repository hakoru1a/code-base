using Shared.DTOs;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Material;

public class MaterialResponseDto : BaseResponseDto<int>
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? ProposedImpurityDeduction { get; set; }
    public EntityStatus Status { get; set; }
}
