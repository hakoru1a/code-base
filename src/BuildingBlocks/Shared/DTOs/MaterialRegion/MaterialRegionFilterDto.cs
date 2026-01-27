using Shared.DTOs;

namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
}
