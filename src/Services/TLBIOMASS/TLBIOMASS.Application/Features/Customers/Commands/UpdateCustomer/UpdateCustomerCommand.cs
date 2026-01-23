using MediatR;

namespace TLBIOMASS.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public string? DienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? Email { get; set; }
    public string? MaSoThue { get; set; }
    public string? GhiChu { get; set; }
    public bool IsActive { get; set; } = true;
}
