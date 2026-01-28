namespace Shared.DTOs.MaterialRegion;

public class MaterialRegionUpdateDto
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal AreaHa { get; set; }
    public string? CertificateID { get; set; }
    public int OwnerId { get; set; }
    public List<RegionMaterialCreateUpdateDto> RegionMaterials { get; set; } = new();
}
