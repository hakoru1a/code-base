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

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new MaterialRegionSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.OwnerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == request.Filter.OwnerId.Value);
        }

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<MaterialRegionResponseDto>(
            pagedItems.Adapt<List<MaterialRegionResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }


}
