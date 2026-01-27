using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;
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

        if (!string.IsNullOrEmpty(request.Search))
        {
            var spec = new MaterialRegionSearchSpecification(request.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.OwnerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Page, request.Size, cancellationToken);

        return new PagedList<MaterialRegionResponseDto>(
            pagedItems.Adapt<List<MaterialRegionResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Page, request.Size);
    }

    private IQueryable<MaterialRegion> ApplySorting(IQueryable<MaterialRegion> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "regionname" => isDescending ? query.OrderByDescending(x => x.RegionName) : query.OrderBy(x => x.RegionName),
            "areaha" => isDescending ? query.OrderByDescending(x => x.AreaHa) : query.OrderBy(x => x.AreaHa),
            "createddate" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
