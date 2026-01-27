using Shared.DTOs;

namespace Shared.DTOs.Landowner;

public class LandownerFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
