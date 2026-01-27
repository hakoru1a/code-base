using Shared.DTOs;

namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionPagedFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
}
