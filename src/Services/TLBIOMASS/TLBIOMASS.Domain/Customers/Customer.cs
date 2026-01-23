using Contracts.Domain;
using Contracts.Domain.Interface;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Rules;

namespace TLBIOMASS.Domain.Customers;

public class Customer : EntityBase<int>
{
    public string TenKhachHang { get; private set; } = string.Empty;
    public string? DienThoai { get; private set; }
    public string? DiaChi { get; private set; }
    public string? GhiChu { get; private set; }
    public string? Email { get; private set; }
    public string? MaSoThue { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Protected constructor for EF Core
    protected Customer() { }

    // Domain constructor
    public Customer(
        string tenKhachHang,
        string? dienThoai = null,
        string? diaChi = null,
        string? email = null,
        string? maSoThue = null,
        string? ghiChu = null)
    {
        TenKhachHang = tenKhachHang;
        DienThoai = dienThoai;
        DiaChi = diaChi;
        Email = email;
        MaSoThue = maSoThue;
        GhiChu = ghiChu;
        IsActive = true;
    }

    public static Customer Create(
        string tenKhachHang,
        string? dienThoai = null,
        string? diaChi = null,
        string? email = null,
        string? maSoThue = null,
        string? ghiChu = null)
    {
        CheckRule(new CustomerNameRequiredRule(tenKhachHang));
        CheckRule(new CustomerEmailFormatRule(email));

        return new Customer(tenKhachHang, dienThoai, diaChi, email, maSoThue, ghiChu);
    }

    public void CheckMaSoThueUnique(ICustomerRepository repository)
    {
        CheckRule(new CustomerMaSoThueUniqueRule(repository, MaSoThue, Id == 0 ? null : Id));
    }

    public void Update(
        string tenKhachHang,
        string? dienThoai,
        string? diaChi,
        string? email,
        string? maSoThue,
        string? ghiChu)
    {
        CheckRule(new CustomerNameRequiredRule(tenKhachHang));
        CheckRule(new CustomerEmailFormatRule(email));

        TenKhachHang = tenKhachHang;
        DienThoai = dienThoai;
        DiaChi = diaChi;
        Email = email;
        MaSoThue = maSoThue;
        GhiChu = ghiChu;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

}
