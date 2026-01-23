using FluentValidation;

namespace TLBIOMASS.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.TenKhachHang)
            .NotEmpty().WithMessage("Tên khách hàng không được để trống")
            .MaximumLength(200).WithMessage("Tên khách hàng không được vượt quá 200 ký tự");

        RuleFor(x => x.DienThoai)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự")
            .When(x => !string.IsNullOrEmpty(x.DienThoai));

        RuleFor(x => x.DiaChi)
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.DiaChi));

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.MaSoThue)
            .MaximumLength(50).WithMessage("Mã số thuế không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.MaSoThue));
    }
}
