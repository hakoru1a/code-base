using Shared.DTOs;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for Order response
/// </summary>
public class OrderResponseDto : BaseResponseDto<long>
{
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
}

