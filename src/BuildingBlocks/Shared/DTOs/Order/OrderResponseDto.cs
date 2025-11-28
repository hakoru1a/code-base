using Shared.DTOs;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for Order response
/// </summary>
public class OrderResponseDto : BaseResponseDto<long>
{
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();

    /// <summary>
    /// Business properties
    /// </summary>
    public bool HasOrderItems { get; set; }
    public int TotalItemsCount { get; set; }
    public int UniqueProductsCount { get; set; }
    public bool IsLargeOrder { get; set; }
    public decimal TotalOrderValue { get; set; }
    public bool CanBeDeleted { get; set; }
}

