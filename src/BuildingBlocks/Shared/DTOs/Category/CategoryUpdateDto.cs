using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Category;

/// <summary>
/// DTO for updating an existing Category
/// </summary>
public class CategoryUpdateDto : BaseUpdateDto<long>
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
}

