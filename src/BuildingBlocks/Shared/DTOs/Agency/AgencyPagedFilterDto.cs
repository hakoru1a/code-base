using Contracts.Domain.Enums;

namespace Shared.DTOs.Agency;

public class AgencyPagedFilterDto : BaseFilterDto
{
    public string? Name { get; set; }
    public EntityStatus? Status { get; set; }
}
