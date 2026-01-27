using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Application.Features.Landowners.DTOs;
using Shared.SeedWork;
using Mapster;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Specifications;
using System.Linq;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQueryHandler : IRequestHandler<GetLandownersQuery, PagedList<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetLandownersQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<LandownerResponseDto>> Handle(GetLandownersQuery request, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = _repository.FindAll();

        // Apply filters using Specifications
        if (!string.IsNullOrEmpty(request.Search))
        {
            var spec = new LandownerSearchSpecification(request.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.IsActive.HasValue)
        {
            var spec = new LandownerIsActiveSpecification(request.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Page, request.Size, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<LandownerResponseDto>(
            pagedItems.Adapt<List<LandownerResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Page, request.Size);
    }

    private IQueryable<Landowner> ApplySorting(IQueryable<Landowner> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "phone" => isDescending ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
            "email" => isDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "bankaccount" => isDescending ? query.OrderByDescending(x => x.BankAccount) : query.OrderBy(x => x.BankAccount),
            "createddate" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" or "updated" => isDescending ? query.OrderByDescending(x => x.LastModifiedDate) : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
