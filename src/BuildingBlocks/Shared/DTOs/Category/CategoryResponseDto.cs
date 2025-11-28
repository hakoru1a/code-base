using Shared.DTOs;

namespace Shared.DTOs.Category;

/// <summary>
/// DTO for Category response
/// </summary>
public class CategoryResponseDto : BaseResponseDto<long>
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of products in this category
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Indicates if this category can be deleted (no products)
    /// </summary>
    public bool CanBeDeleted { get; set; }
}

