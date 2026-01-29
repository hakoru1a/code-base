using Shared.SeedWork;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Receiver;

public class ReceiverFilterDto : RequestParameter
{
    public EntityStatus? Status { get; set; }
}
