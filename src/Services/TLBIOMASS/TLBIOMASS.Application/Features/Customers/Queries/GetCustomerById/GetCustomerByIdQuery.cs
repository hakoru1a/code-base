using MediatR;
using Shared.DTOs.Customer;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQuery : IRequest<CustomerResponseDto?>
{
    public int Id { get; set; }
}
