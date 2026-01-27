using Mapster;
using MediatR;
using Shared.DTOs.Customer;
using TLBIOMASS.Domain.Customers.Interfaces;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponseDto?>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerResponseDto?> Handle(
        GetCustomerByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (customer == null)
            return null;

        return customer.Adapt<CustomerResponseDto>();
    }
}
