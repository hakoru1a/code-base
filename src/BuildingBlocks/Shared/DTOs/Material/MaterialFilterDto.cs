using Shared.SeedWork;

namespace Shared.DTOs.Material;

public class MaterialFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
