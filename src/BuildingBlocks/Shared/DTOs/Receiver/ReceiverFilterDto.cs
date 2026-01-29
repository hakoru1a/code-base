using Shared.SeedWork;

namespace Shared.DTOs.Receiver;

public class ReceiverFilterDto : RequestParameter
{
    public bool? IsActive { get; set; }
}
