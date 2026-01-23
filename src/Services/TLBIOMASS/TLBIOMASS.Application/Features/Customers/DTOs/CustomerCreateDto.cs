namespace TLBIOMASS.Application.Features.Customers.DTOs;

public class CustomerCreateDto
{
    public string TenKhachHang { get; set; } = string.Empty;
    public string? DienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? Email { get; set; }
    public string? MaSoThue { get; set; }
    public string? GhiChu { get; set; }
}
