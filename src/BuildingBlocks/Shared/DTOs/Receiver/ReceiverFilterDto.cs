using Shared.SeedWork;

namespace Shared.DTOs.Receiver;

public class ReceiverFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
