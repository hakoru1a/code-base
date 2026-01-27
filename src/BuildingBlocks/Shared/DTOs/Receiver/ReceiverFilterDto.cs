using Shared.DTOs;

namespace Shared.DTOs.Receiver;

public class ReceiverFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
