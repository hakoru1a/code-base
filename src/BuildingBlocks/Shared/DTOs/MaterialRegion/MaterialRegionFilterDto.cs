using Shared.SeedWork;

namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionFilterDto : RequestParameter
{
    public int? OwnerId { get; set; }
}
