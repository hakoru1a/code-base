using Contracts.Domain;
using TLBIOMASS.Domain.Materials;

namespace TLBIOMASS.Domain.MaterialRegions;

public class RegionMaterial : EntityAuditBase<int>
{
    public int MaterialId { get; private set; }
    public int MaterialRegionId { get; private set; }
    public double? AreaHa { get; private set; } // Optional: area for this specific material

    // Navigation properties
    public virtual MaterialRegion MaterialRegion { get; private set; } = null!;
    public virtual Material Material { get; private set; } = null!;

    private RegionMaterial() { }

    internal RegionMaterial(MaterialRegion materialRegion, Material material, double? areaHa = null)
    {
        MaterialRegion = materialRegion;
        MaterialRegionId = materialRegion.Id;
        Material = material;
        MaterialId = material.Id;
        AreaHa = areaHa;
        CreatedDate = DateTime.UtcNow;
    }

    public void UpdateArea(double? areaHa)
    {
        AreaHa = areaHa;
    }
}
