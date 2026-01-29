using Shared.DTOs;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Landowner;

public class LandownerPagedFilterDto : BaseFilterDto
{
    public EntityStatus? Status { get; set; }
}
