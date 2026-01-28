using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.Receivers.Specifications;
using Mapster;
using System.Linq;
using TLBIOMASS.Domain.Receivers;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQueryHandler : IRequestHandler<GetReceiversQuery, PagedList<ReceiverResponseDto>>
{
    private readonly IReceiverRepository _repository;

    public GetReceiversQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<ReceiverResponseDto>> Handle(GetReceiversQuery request, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = _repository.FindAll();

        // Apply filters using Specifications as per Architecture Guide
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new ReceiverSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new ReceiverIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<ReceiverResponseDto>(
            pagedItems.Adapt<List<ReceiverResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }

}
