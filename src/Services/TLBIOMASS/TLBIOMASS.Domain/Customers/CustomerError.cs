using Contracts.Exceptions;

namespace TLBIOMASS.Domain.Customers;

public static class CustomerError
{
    public static BusinessException TenKhachHangCannotBeEmpty()
        => new("Tên khách hàng không được để trống");

    public static BusinessException TenKhachHangTooLong(int maxLength = 200)
        => new($"Tên khách hàng không được vượt quá {maxLength} ký tự");

    public static BusinessException EmailInvalidFormat()
        => new("Email không đúng định dạng");

    public static BusinessException MaSoThueAlreadyExists(string maSoThue)
        => new($"Mã số thuế '{maSoThue}' đã tồn tại");
}
