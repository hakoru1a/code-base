using Shared.SeedWork;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Landowner;

public class LandownerFilterDto : RequestParameter
{
    public EntityStatus? Status { get; set; }
}
