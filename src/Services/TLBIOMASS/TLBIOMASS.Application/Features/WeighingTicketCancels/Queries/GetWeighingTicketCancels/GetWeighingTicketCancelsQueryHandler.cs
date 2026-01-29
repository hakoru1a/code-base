using MediatR;
using Shared.SeedWork;
using Shared.DTOs.WeighingTicketCancel;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using Mapster;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancels;

public class GetWeighingTicketCancelsQueryHandler : IRequestHandler<GetWeighingTicketCancelsQuery, PagedList<WeighingTicketCancelResponseDto>>
{
    private readonly IWeighingTicketCancelRepository _repository;

    public GetWeighingTicketCancelsQueryHandler(IWeighingTicketCancelRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<WeighingTicketCancelResponseDto>> Handle(GetWeighingTicketCancelsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (request.Filter.WeighingTicketId.HasValue)
        {
            query = query.Where(x => x.WeighingTicketId == request.Filter.WeighingTicketId.Value);
        }

        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var term = request.Filter.SearchTerms.ToLower();
            query = query.Where(x => x.CancelReason != null && x.CancelReason.ToLower().Contains(term));
        }

        query = query.OrderByDescending(x => x.CreatedDate);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);
        
        return new PagedList<WeighingTicketCancelResponseDto>(
            pagedItems.Adapt<List<WeighingTicketCancelResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }
}
