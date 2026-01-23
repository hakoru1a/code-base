using MediatR;
using TLBIOMASS.Application.Features.Customers.DTOs;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQuery : IRequest<CustomerResponseDto?>
{
    public int Id { get; set; }
}
