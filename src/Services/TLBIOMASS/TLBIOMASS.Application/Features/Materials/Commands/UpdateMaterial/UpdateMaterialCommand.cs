using MediatR;

namespace TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial;

public class UpdateMaterialCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public bool IsActive { get; set; }
}
