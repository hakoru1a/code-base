using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Specifications;

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
        var query = _repository.FindAll();

        query = ApplyFilter(query, request.Filter);

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new LandownerSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new LandownerIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<LandownerResponseDto>(
            pagedItems.Adapt<List<LandownerResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Landowner> ApplyFilter(IQueryable<Landowner> query, LandownerPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        return query;
    }

    private static IQueryable<Landowner> ApplySort(IQueryable<Landowner> query, string? orderBy, string? direction)
    {
        return query;
    }
}
