using Shared.DTOs;

namespace Shared.DTOs.Category;

/// <summary>
/// DTO for deleting a Category
/// </summary>
public class CategoryDeleteDto : BaseDeleteDto<long>
{
    // Additional properties can be added if needed
    // For example: SoftDelete flag, DeleteReason, etc.
}

