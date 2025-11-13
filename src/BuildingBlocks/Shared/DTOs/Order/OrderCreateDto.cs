using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for creating a new Order
/// </summary>
public class OrderCreateDto : BaseCreateDto
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 200 characters")]
    public string CustomerName { get; set; } = string.Empty;

    public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();
}

