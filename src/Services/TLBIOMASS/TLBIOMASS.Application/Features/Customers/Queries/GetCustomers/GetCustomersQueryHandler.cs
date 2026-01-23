using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Customers.DTOs;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Specifications;
using TLBIOMASS.Application.Common.Extensions;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;


public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedList<CustomerResponseDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedList<CustomerResponseDto>> Handle(
        GetCustomersQuery request, 
        CancellationToken cancellationToken)
    {
        // Build query
        var query = _customerRepository.FindAll();

        // Filter by search term using Specification
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchSpec = new CustomerSearchSpecification(request.Search);
            query = query.Where(searchSpec.ToExpression());
        }

        // Filter by IsActive using Specification
        if (request.IsActive.HasValue)
        {
            var activeSpec = new CustomerIsActiveSpecification(request.IsActive.Value);
            query = query.Where(activeSpec.ToExpression());
        }

        // Apply Sorting dynamically
        query = query.ApplySorting(request.SortBy, request.SortDirection);

        // Get paginated results using repository base method
        var pagedItems = await _customerRepository.GetPageAsync(
            query,
            request.Page,
            request.Size,
            cancellationToken);

        // Map to DTO
        var dtos = pagedItems.Adapt<List<CustomerResponseDto>>();

        return new PagedList<CustomerResponseDto>(
            dtos,
            pagedItems.GetMetaData().TotalItems,
            request.Page,
            request.Size
        );
    }

}
