using Shared.DTOs.Customer;
using MediatR;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomersPaged;

public class GetCustomersPagedQuery : IRequest<PagedList<CustomerResponseDto>>
{
    public CustomerPagedFilterDto Filter { get; set; } = new();
}
