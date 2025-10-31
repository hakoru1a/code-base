using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Generate.Application.Common.DTOs.Category
{
    /// <summary>
    /// DTO for creating a new Category
    /// </summary>
    public class CategoryCreateDto : BaseCreateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}

