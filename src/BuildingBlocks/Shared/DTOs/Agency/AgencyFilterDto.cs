using Shared.SeedWork;

namespace Shared.DTOs.Agency;

public class AgencyFilterDto : RequestParameter
{
    public string? Name { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
