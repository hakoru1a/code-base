using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for creating a new OrderItem
/// </summary>
public class OrderItemCreateDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

