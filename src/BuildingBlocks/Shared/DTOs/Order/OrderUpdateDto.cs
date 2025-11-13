using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for updating an existing Order
/// </summary>
public class OrderUpdateDto : BaseUpdateDto<long>
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 200 characters")]
    public string CustomerName { get; set; } = string.Empty;

    public List<OrderItemUpdateDto> OrderItems { get; set; } = new List<OrderItemUpdateDto>();
}

