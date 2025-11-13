using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for updating an existing OrderItem
/// </summary>
public class OrderItemUpdateDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Product ID is required")]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

