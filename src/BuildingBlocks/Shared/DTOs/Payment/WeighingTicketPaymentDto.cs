namespace Shared.DTOs.Payment;

public class WeighingTicketPaymentDto
{
    public int WeighingTicketId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPayableAmount { get; set; }
    public string? Note { get; set; }
}
