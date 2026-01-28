using System.Linq;
using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Shared.DTOs.MaterialRegion;
using Shared.SeedWork;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.MaterialRegions.Specifications;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;

public class GetMaterialRegionsQueryHandler : IRequestHandler<GetMaterialRegionsQuery, PagedList<MaterialRegionResponseDto>>
{
    private readonly IMaterialRegionRepository _repository;

    public GetMaterialRegionsQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<MaterialRegionResponseDto>> Handle(GetMaterialRegionsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll()
            .Include(x => x.RegionMaterials)
            .ThenInclude(x => x.Material)
            .AsQueryable();

        query = ApplyFilter(query, request.Filter);

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new MaterialRegionSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<MaterialRegionResponseDto>(
            pagedItems.Adapt<List<MaterialRegionResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<MaterialRegion> ApplyFilter(IQueryable<MaterialRegion> query, MaterialRegionPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.OwnerId.HasValue)
            query = query.Where(x => x.OwnerId == filter.OwnerId.Value);

        return query;
    }

    private static IQueryable<MaterialRegion> ApplySort(IQueryable<MaterialRegion> query, string? orderBy, string? direction)
    {
        return query;
    }
}
