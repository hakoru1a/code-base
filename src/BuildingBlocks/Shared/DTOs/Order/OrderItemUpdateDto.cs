using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Order;

/// <summary>
/// DTO for updating an existing OrderItem - uses composite key
/// </summary>
public class OrderItemUpdateDto
{
    /// <summary>
    /// Composite key - identifies the order item to update
    /// </summary>
    [Required(ErrorMessage = "Product ID is required")]
    public long ProductId { get; set; }

    /// <summary>
    /// New quantity value
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

