using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Shared.DTOs.MaterialRegion;
using TLBIOMASS.Domain.MaterialRegions.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetAllMaterialRegions;

public class GetAllMaterialRegionsQueryHandler : IRequestHandler<GetAllMaterialRegionsQuery, List<MaterialRegionResponseDto>>
{
    private readonly IMaterialRegionRepository _repository;

    public GetAllMaterialRegionsQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaterialRegionResponseDto>> Handle(GetAllMaterialRegionsQuery request, CancellationToken cancellationToken)
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

        query = query.OrderByDescending(x => x.CreatedDate);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<MaterialRegionResponseDto>>();
    }
}
