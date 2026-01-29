namespace Shared.DTOs.Payment;

public class PaymentGroupResponseDto
{
    public string PaymentCode { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public decimal TotalAmount { get; set; }
    public int TicketCount { get; set; }
}
