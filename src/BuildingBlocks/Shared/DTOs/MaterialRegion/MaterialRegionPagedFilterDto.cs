using Shared.DTOs;

namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionPagedFilterDto : BaseFilterDto
{
    public int? OwnerId { get; set; }
}
