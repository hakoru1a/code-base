namespace Generate.Application.Common.DTOs.Order
{
    /// <summary>
    /// DTO for OrderItem response
    /// </summary>
    public class OrderItemResponseDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string? ProductName { get; set; }
    }
}

