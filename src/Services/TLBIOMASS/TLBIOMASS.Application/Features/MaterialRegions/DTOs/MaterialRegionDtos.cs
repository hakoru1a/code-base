namespace TLBIOMASS.Application.Features.MaterialRegions.DTOs;

public class MaterialRegionResponseDto
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal AreaHa { get; set; }
    public string? CertificateID { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public List<RegionMaterialDto> RegionMaterials { get; set; } = new();
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
}

public class RegionMaterialDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public double? AreaHa { get; set; }
}

public class MaterialRegionCreateDto
{
    public string RegionName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal AreaHa { get; set; }
    public string? CertificateID { get; set; }
    public int OwnerId { get; set; }
    public List<RegionMaterialCreateUpdateDto> RegionMaterials { get; set; } = new();
}

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

public class RegionMaterialCreateUpdateDto
{
    public int MaterialId { get; set; }
    public double? AreaHa { get; set; }
}

public class MaterialRegionFilterDto
{
    public string? Search { get; set; }
    public int? OwnerId { get; set; }
}
