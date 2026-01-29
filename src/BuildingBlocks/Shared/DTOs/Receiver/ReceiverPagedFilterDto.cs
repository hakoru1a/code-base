using Shared.DTOs;

namespace Shared.DTOs.Receiver;

public class ReceiverPagedFilterDto : BaseFilterDto
{
    public bool? IsActive { get; set; }
}
