using Shared.DTOs;

namespace Shared.DTOs.Category;

/// <summary>
/// DTO for Category response
/// </summary>
public class CategoryResponseDto : BaseResponseDto<long>
{
    public string Name { get; set; } = string.Empty;

    // Add any additional properties specific to Category response
    // public int ProductCount { get; set; }
}

