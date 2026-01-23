using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Application.Features.Customers.DTOs;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Specifications;
using TLBIOMASS.Application.Common.Extensions;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerResponseDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponseDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _customerRepository.FindAll();

        // Apply Search
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchSpec = new CustomerSearchSpecification(request.Search);
            query = query.Where(searchSpec.ToExpression());
        }

        // Apply Active Filter
        if (request.IsActive.HasValue)
        {
            var activeSpec = new CustomerIsActiveSpecification(request.IsActive.Value);
            query = query.Where(activeSpec.ToExpression());
        }

        // Apply Sorting dynamically
        query = query.ApplySorting(request.SortBy, request.SortDirection);

        var customers = await query.ToListAsync(cancellationToken);

        return customers.Adapt<List<CustomerResponseDto>>();
    }

}
