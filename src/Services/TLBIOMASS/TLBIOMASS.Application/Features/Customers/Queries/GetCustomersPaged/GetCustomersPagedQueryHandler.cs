using System.Linq;
using Mapster;
using MediatR;
using Shared.DTOs.Customer;
using Shared.SeedWork;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;

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

        query = ApplyFilter(query, filter);

        query = ApplySort(query, filter.OrderBy, filter.OrderByDirection);

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

    private static IQueryable<Customer> ApplyFilter(IQueryable<Customer> query, CustomerPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.TaxCode != null && c.TaxCode.ToLower().Contains(search)));
        }

        return query;
    }

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> query, string? orderBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Id);

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            "taxcode" => isDescending
                ? query.OrderByDescending(x => x.TaxCode)
                : query.OrderBy(x => x.TaxCode),
            "phone" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Phone : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Phone : null),
            "email" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Email : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Email : null),
            "address" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Address : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Address : null),
            "isactive" => isDescending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),
            "createddate" => isDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" => isDescending
                ? query.OrderByDescending(x => x.LastModifiedDate)
                : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderBy(x => x.Id)
        };
    }
}

