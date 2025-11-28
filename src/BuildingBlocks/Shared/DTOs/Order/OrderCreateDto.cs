using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for creating a new Order
/// </summary>
public class OrderCreateDto : BaseCreateDto
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters")]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Initial order items (optional - can be added later)
    /// </summary>
    public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();
}

