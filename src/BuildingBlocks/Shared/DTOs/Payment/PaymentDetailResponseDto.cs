using Shared.DTOs;
using Shared.DTOs.Agency;

namespace Shared.DTOs.Payment;

public class PaymentDetailResponseDto : BaseResponseDto<int>
{
    public int WeighingTicketId { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int? AgencyId { get; set; }
    public AgencyResponseDto? Agency { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? Note { get; set; }
    public bool IsPaid { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? CustomerPaymentDate { get; set; }
    
    // Joined info
    public string? TicketNumber { get; set; }
}
