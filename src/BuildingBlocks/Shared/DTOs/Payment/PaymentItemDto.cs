namespace Shared.DTOs.Payment;

public class PaymentItemDto
{
    public int WeighingTicketId { get; set; }
    public int? AgencyId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? CustomerPaymentDate { get; set; }

    // Optional: Set final price/total during first payment
    public decimal? FinalUnitPrice { get; set; }
    public decimal? FinalTotalPayableAmount { get; set; }
}
