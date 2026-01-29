using Shared.DTOs;

namespace Shared.DTOs.WeighingTicketCancel;

public class WeighingTicketCancelResponseDto : BaseResponseDto<int>
{
    public int WeighingTicketId { get; set; }
    public string? CancelReason { get; set; }
}
