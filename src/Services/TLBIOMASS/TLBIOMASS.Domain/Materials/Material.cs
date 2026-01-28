using Contracts.Domain;
using TLBIOMASS.Domain.Materials.ValueObjects;

namespace TLBIOMASS.Domain.Materials;

public class Material : EntityBase<int>
{
    public MaterialSpec Spec { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected Material() { }

    private Material(MaterialSpec spec, bool isActive)
    {
        Spec = spec;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public static Material Create(MaterialSpec spec, bool isActive = true)
    {
        return new Material(spec, isActive);
    }

    public void Update(MaterialSpec spec, bool isActive)
    {
        Spec = spec;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
