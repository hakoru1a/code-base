using MediatR;
using TLBIOMASS.Application.Features.Customers.DTOs;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQuery : IRequest<List<CustomerResponseDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
