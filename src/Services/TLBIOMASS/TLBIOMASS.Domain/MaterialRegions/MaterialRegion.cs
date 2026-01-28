using Contracts.Domain;
using TLBIOMASS.Domain.Materials;

namespace TLBIOMASS.Domain.MaterialRegions;

public class MaterialRegion : EntityAuditBase<int>
{
    public string RegionName { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public decimal AreaHa { get; private set; }
    public string? CertificateID { get; private set; }
    public int OwnerId { get; private set; }

    private readonly List<RegionMaterial> _regionMaterials = new();
    public virtual IReadOnlyList<RegionMaterial> RegionMaterials => _regionMaterials.AsReadOnly();

    // Protected constructor for EF Core
    protected MaterialRegion() { }

    private MaterialRegion(
        string regionName,
        string? address,
        double latitude,
        double longitude,
        decimal areaHa,
        string? certificateID,
        int ownerId)
    {
        RegionName = regionName;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        AreaHa = areaHa;
        CertificateID = certificateID;
        OwnerId = ownerId;
    }

    // Factory Method
    public static MaterialRegion Create(
        string regionName,
        string? address,
        double latitude,
        double longitude,
        decimal areaHa,
        string? certificateID,
        int ownerId)
    {
        return new MaterialRegion(
            regionName,
            address,
            latitude,
            longitude,
            areaHa,
            certificateID,
            ownerId);
    }

    // Business Methods
    public void Update(
        string regionName,
        string? address,
        double latitude,
        double longitude,
        decimal areaHa,
        string? certificateID,
        int ownerId)
    {
        RegionName = regionName;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        AreaHa = areaHa;
        CertificateID = certificateID;
        OwnerId = ownerId;
    }

    public void AddMaterial(int materialId, double? areaHa = null)
    {
        if (_regionMaterials.Any(x => x.MaterialId == materialId))
            return;

        _regionMaterials.Add(new RegionMaterial(this, materialId, areaHa));
    }

    public void RemoveMaterial(int materialId)
    {
        var item = _regionMaterials.FirstOrDefault(x => x.MaterialId == materialId);
        if (item != null)
        {
            _regionMaterials.Remove(item);
        }
    }

    public void ClearMaterials()
    {
        _regionMaterials.Clear();
    }
}
