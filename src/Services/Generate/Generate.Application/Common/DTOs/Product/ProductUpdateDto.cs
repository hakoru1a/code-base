using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Generate.Application.Common.DTOs.Product
{
    /// <summary>
    /// DTO for updating an existing Product
    /// </summary>
    public class ProductUpdateDto : BaseUpdateDto<long>
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        public long? CategoryId { get; set; }
    }
}

