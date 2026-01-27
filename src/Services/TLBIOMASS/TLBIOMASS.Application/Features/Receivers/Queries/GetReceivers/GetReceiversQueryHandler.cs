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

        // Apply sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<ReceiverResponseDto>(
            pagedItems.Adapt<List<ReceiverResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }

    private IQueryable<Receiver> ApplySorting(IQueryable<Receiver> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "phone" => isDescending ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
            "bankaccount" => isDescending ? query.OrderByDescending(x => x.BankAccount) : query.OrderBy(x => x.BankAccount),
            "createdat" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" or "updated" => isDescending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
