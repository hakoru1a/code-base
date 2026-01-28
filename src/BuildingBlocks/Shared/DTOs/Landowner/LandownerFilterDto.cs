using Shared.SeedWork;

namespace Shared.DTOs.Landowner;

public class LandownerFilterDto : RequestParameter
{
    public bool? IsActive { get; set; }
}
