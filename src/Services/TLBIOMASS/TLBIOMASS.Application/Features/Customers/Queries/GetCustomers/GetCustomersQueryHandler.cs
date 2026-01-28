using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Customer;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Specifications;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerResponseDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponseDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _customerRepository.FindAll();

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchSpec = new CustomerSearchSpecification(filter.Search);
            query = query.Where(searchSpec.ToExpression());
        }

        if (filter.IsActive.HasValue)
        {
            var activeSpec = new CustomerIsActiveSpecification(filter.IsActive.Value);
            query = query.Where(activeSpec.ToExpression());
        }

        var customers = await query.ToListAsync(cancellationToken);

        return customers.Adapt<List<CustomerResponseDto>>();
    }
}
