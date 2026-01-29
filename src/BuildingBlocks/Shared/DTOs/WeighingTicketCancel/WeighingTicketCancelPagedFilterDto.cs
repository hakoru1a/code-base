using Shared.DTOs;

namespace Shared.DTOs.WeighingTicketCancel;

public class WeighingTicketCancelPagedFilterDto : BaseFilterDto
{
    // SearchTerms from base
    public int? WeighingTicketId { get; set; }
}
