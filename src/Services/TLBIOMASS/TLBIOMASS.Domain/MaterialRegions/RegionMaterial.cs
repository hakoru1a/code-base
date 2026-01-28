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
    public virtual Material? Material { get; private set; } // Make nullable to avoid required check on insert

    protected RegionMaterial() { }

    internal RegionMaterial(MaterialRegion materialRegion, int materialId, double? areaHa = null)
    {
        MaterialRegion = materialRegion;
        // MaterialRegionId might not be set yet if MaterialRegion is new (Id=0), 
        // but EF Core handles the relationship fixup via navigation property.
        
        MaterialId = materialId;
        AreaHa = areaHa;
        CreatedDate = DateTime.UtcNow;
    }

    public void UpdateArea(double? areaHa)
    {
        AreaHa = areaHa;
    }
}
