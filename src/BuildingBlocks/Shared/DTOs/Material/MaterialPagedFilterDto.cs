using Shared.DTOs;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Material;

public class MaterialPagedFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public EntityStatus? Status { get; set; }
}
