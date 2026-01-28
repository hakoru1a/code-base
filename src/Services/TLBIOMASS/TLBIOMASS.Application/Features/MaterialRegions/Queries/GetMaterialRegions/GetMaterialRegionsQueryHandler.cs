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
            var spec = new MaterialRegionSearchSpecification(request.Filter.SearchTerms);
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
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Id);

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "regionname" => isDescending
                ? query.OrderByDescending(x => x.Detail.RegionName)
                : query.OrderBy(x => x.Detail.RegionName),
            "address" => isDescending
                ? query.OrderByDescending(x => x.Detail.Address)
                : query.OrderBy(x => x.Detail.Address),
            "areaha" => isDescending
                ? query.OrderByDescending(x => x.Detail.AreaHa)
                : query.OrderBy(x => x.Detail.AreaHa),
            "certificateid" => isDescending
                ? query.OrderByDescending(x => x.Detail.CertificateId)
                : query.OrderBy(x => x.Detail.CertificateId),
            "ownerid" => isDescending
                ? query.OrderByDescending(x => x.OwnerId)
                : query.OrderBy(x => x.OwnerId),
            "createddate" => isDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" => isDescending
                ? query.OrderByDescending(x => x.LastModifiedDate)
                : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
