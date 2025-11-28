namespace Shared.DTOs.Order;

/// <summary>
/// DTO for OrderItem response - follows DDD composite key structure
/// </summary>
public class OrderItemResponseDto
{
    /// <summary>
    /// Composite key properties
    /// </summary>
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    
    /// <summary>
    /// Order item properties
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Navigation properties for display
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Business properties
    /// </summary>
    public bool IsLargeOrder { get; set; }
    public decimal TotalValue { get; set; }
}

