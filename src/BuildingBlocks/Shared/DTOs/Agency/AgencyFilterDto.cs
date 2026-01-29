using Contracts.Domain.Enums;
using Shared.SeedWork;

namespace Shared.DTOs.Agency;

public class AgencyFilterDto : RequestParameter
{
    public string? Name { get; set; }
    public EntityStatus? Status { get; set; }
}
