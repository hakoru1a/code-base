using Contracts.Domain;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;

namespace TLBIOMASS.Domain.MaterialRegions;

public class MaterialRegion : EntityAuditBase<int>
{
    public RegionDetail Detail { get; private set; } = null!;
    public int OwnerId { get; private set; }

    private readonly List<RegionMaterial> _regionMaterials = new();
    public virtual IReadOnlyList<RegionMaterial> RegionMaterials => _regionMaterials.AsReadOnly();

    protected MaterialRegion() { }

    private MaterialRegion(RegionDetail detail, int ownerId)
    {
        Detail = detail;
        OwnerId = ownerId;
    }

    public static MaterialRegion Create(RegionDetail detail, int ownerId)
    {
        return new MaterialRegion(detail, ownerId);
    }

    public void Update(RegionDetail detail, int ownerId)
    {
        Detail = detail;
        OwnerId = ownerId;
    }

    public void AddMaterial(Material material, double? areaHa = null)
    {
        if (material == null) throw new ArgumentNullException(nameof(material));

        if (_regionMaterials.Any(x => x.MaterialId == material.Id))
            return;

        _regionMaterials.Add(new RegionMaterial(this, material, areaHa));
    }

    public void RemoveMaterial(int materialId)
    {
        var item = _regionMaterials.FirstOrDefault(x => x.MaterialId == materialId);
        if (item != null)
            _regionMaterials.Remove(item);
    }

    public void ClearMaterials()
    {
        _regionMaterials.Clear();
    }
}
