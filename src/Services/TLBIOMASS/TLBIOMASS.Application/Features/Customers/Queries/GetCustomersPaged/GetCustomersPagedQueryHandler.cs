using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Customer;
using Shared.SeedWork;
using Shared.Extensions;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Specifications;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomersPaged;

public class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, PagedList<CustomerResponseDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersPagedQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedList<CustomerResponseDto>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _customerRepository.FindAll();

        // Auto filters (simple filters)
        query = query.ApplyFilters(filter);

        // Business logic (Specifications)
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

        // Sorting
        query = query.ApplySort(filter.OrderBy, filter.OrderByDirection);

        var pagedItems = await _customerRepository.GetPageAsync(
            query,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var dtos = pagedItems.Adapt<List<CustomerResponseDto>>();

        return new PagedList<CustomerResponseDto>(
            dtos,
            pagedItems.GetMetaData().TotalItems,
            filter.PageNumber,
            filter.PageSize
        );
    }
}

