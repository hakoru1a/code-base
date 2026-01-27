using MediatR;
using TLBIOMASS.Application.Features.Receivers.DTOs;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetAllReceivers;

public class GetAllReceiversQuery : IRequest<List<ReceiverResponseDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
