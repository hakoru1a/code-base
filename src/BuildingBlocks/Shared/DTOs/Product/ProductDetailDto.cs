using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product;

/// <summary>
/// DTO for ProductDetail information
/// </summary>
public class ProductDetailDto
{
    [Required(ErrorMessage = "Product description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Product description must be between 10 and 1000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Summary of the description for display purposes
    /// </summary>
    public string? DescriptionSummary { get; set; }

    /// <summary>
    /// Indicates if the product has a detailed description
    /// </summary>
    public bool HasDescription { get; set; }
}
