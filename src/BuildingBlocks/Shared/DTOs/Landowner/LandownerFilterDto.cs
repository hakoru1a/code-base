using Shared.SeedWork;

namespace Shared.DTOs.Landowner;

public class LandownerFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
