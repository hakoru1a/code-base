using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Customers.DTOs;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQuery : IRequest<PagedList<CustomerResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
