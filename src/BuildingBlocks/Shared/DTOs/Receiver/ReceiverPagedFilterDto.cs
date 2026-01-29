using Shared.DTOs;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Receiver;

public class ReceiverPagedFilterDto : BaseFilterDto
{
    public EntityStatus? Status { get; set; }
}
