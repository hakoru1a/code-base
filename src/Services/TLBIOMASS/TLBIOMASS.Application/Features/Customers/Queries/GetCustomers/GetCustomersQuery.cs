using Shared.DTOs.Customer;
using MediatR;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQuery : IRequest<List<CustomerResponseDto>>
{
    public CustomerFilterDto Filter { get; set; } = new();
}
