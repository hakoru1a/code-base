using Shared.DTOs;

namespace Shared.DTOs.Material;

public class MaterialPagedFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
