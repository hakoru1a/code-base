using Shared.DTOs;

namespace Shared.DTOs.Product;

/// <summary>
/// DTO for Product response - follows DDD entity structure
/// </summary>
public class ProductDto : BaseResponseDto<long>
{
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category information
    /// </summary>
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// Product detail information
    /// </summary>
    public ProductDetailDto? ProductDetail { get; set; }
    
    /// <summary>
    /// Business properties
    /// </summary>
    public bool IsInCategory { get; set; }
    public int OrderItemsCount { get; set; }
    public decimal TotalOrderedQuantity { get; set; }
    public bool CanBeDeleted { get; set; }
}

