using MediatR;
using Contracts.Domain.Enums;

namespace TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial;

public class UpdateMaterialCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public EntityStatus Status { get; set; }
}
