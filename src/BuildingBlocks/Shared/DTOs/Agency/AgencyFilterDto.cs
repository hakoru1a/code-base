using Shared.DTOs;

namespace Shared.DTOs.Agency;

public class AgencyFilterDto : BaseFilterDto
{
    public string? Name { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
