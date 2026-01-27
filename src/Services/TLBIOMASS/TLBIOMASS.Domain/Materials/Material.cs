using Contracts.Domain;
using TLBIOMASS.Domain.Materials.Rules;

namespace TLBIOMASS.Domain.Materials;

public class Material : EntityBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal ProposedImpurityDeduction { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Protected constructor for EF Core
    protected Material() { }

    private Material(
        string name,
        string unit,
        string? description,
        decimal proposedImpurityDeduction,
        bool isActive)
    {
        Name = name;
        Unit = unit;
        Description = description;
        ProposedImpurityDeduction = proposedImpurityDeduction;
        IsActive = isActive;
        CreatedAt = DateTime.Now;
    }

    // Factory Method
    public static Material Create(
        string name,
        string unit,
        string? description,
        decimal proposedImpurityDeduction,
        bool isActive = true)
    {
        CheckRule(new MaterialNameRequiredRule(name));

        return new Material(
            name,
            unit,
            description,
            proposedImpurityDeduction,
            isActive);
    }

    // Update method
    public void Update(
        string name,
        string unit,
        string? description,
        decimal proposedImpurityDeduction,
        bool isActive)
    {
        CheckRule(new MaterialNameRequiredRule(name));

        Name = name;
        Unit = unit;
        Description = description;
        ProposedImpurityDeduction = proposedImpurityDeduction;
        IsActive = isActive;
        UpdatedAt = DateTime.Now;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
