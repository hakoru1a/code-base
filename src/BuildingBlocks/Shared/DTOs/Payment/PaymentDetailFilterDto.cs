using Shared.DTOs;

namespace Shared.DTOs.Payment;

public class PaymentDetailFilterDto : BaseFilterDto
{
    public int? AgencyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsPaid { get; set; }
    public bool? IsLocked { get; set; }
}
