using MediatR;
using TLBIOMASS.Domain.Customers.Events;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;

namespace TLBIOMASS.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMediator _mediator;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IMediator mediator)
    {
        _customerRepository = customerRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Lấy customer từ database
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return false;
        }

        // Sử dụng Domain method để update
        customer.Update(
            request.TenKhachHang,
            request.DienThoai,
            request.DiaChi,
            request.Email,
            request.MaSoThue,
            request.GhiChu
        );

        // Kiểm tra mã số thuế thông qua Domain Rule
        customer.CheckMaSoThueUnique(_customerRepository);

        // Cập nhật trạng thái Active
        if (request.IsActive)
            customer.Activate();
        else
            customer.Deactivate();

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        await _mediator.Publish(new CustomerUpdatedEvent
        {
            CustomerId = customer.Id,
            TenKhachHang = customer.TenKhachHang
        }, cancellationToken);

        return true;
    }
}
