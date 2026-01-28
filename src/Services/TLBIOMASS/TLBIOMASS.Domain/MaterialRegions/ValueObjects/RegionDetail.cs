namespace TLBIOMASS.Domain.MaterialRegions.ValueObjects;

/// <summary>Thông tin vùng: tên, địa chỉ, tọa độ, diện tích, chứng nhận.</summary>
public record RegionDetail(
    string RegionName,
    string? Address,
    double Latitude,
    double Longitude,
    decimal AreaHa,
    string? CertificateId = null)
{
    public GeoLocation Location => new(Latitude, Longitude);
}
