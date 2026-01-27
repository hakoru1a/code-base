using MediatR;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Receivers.DTOs;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQuery : IRequest<PagedList<ReceiverResponseDto>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
