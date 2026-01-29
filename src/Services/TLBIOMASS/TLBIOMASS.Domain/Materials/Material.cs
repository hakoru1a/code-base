using Contracts.Domain;
using Contracts.Domain.Enums;
using TLBIOMASS.Domain.Materials.ValueObjects;

namespace TLBIOMASS.Domain.Materials;

public class Material : EntityBase<int>
{
    public MaterialSpec Spec { get; private set; } = null!;
    // Status (EntityStatus) is inherited from EntityBase.
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected Material() { }

    private Material(MaterialSpec spec, EntityStatus status)
    {
        Spec = spec;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public static Material Create(MaterialSpec spec, EntityStatus status = EntityStatus.Active)
    {
        return new Material(spec, status);
    }

    public void Update(MaterialSpec spec, EntityStatus status)
    {
        Spec = spec;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => Status = EntityStatus.Active;
    public void Deactivate() => Status = EntityStatus.Delete;
}
