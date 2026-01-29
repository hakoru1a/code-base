using Shared.DTOs;

namespace Shared.DTOs.Landowner;

public class LandownerPagedFilterDto : BaseFilterDto
{
    public bool? IsActive { get; set; }
}
