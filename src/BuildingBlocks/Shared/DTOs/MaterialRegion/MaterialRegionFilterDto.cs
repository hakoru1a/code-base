using Shared.SeedWork;

namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
}
