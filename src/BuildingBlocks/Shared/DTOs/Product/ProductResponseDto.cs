using Shared.DTOs;

namespace Shared.DTOs.Product;

/// <summary>
/// DTO for Product response - simplified version of ProductDto
/// </summary>
public class ProductResponseDto : BaseResponseDto<long>
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category information
    /// </summary>
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    /// <summary>
    /// Product detail summary
    /// </summary>
    public string? ProductDetailSummary { get; set; }

    /// <summary>
    /// Business status
    /// </summary>
    public bool IsInCategory { get; set; }
    public bool CanBeDeleted { get; set; }
}

