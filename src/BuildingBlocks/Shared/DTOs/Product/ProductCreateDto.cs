using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product;

/// <summary>
/// DTO for creating a new Product
/// </summary>
public class ProductCreateDto : BaseCreateDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional category ID to assign product to
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// Optional product detail information
    /// </summary>
    public ProductDetailDto? ProductDetail { get; set; }
}

