using Shared.SeedWork;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Material;

public class MaterialFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public EntityStatus? Status { get; set; }
}
