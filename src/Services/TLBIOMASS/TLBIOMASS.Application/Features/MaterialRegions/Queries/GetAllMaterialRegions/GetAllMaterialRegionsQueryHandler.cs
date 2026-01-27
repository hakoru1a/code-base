using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;
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

        if (!string.IsNullOrEmpty(request.Search))
        {
            var spec = new MaterialRegionSearchSpecification(request.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.OwnerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        query = query.OrderByDescending(x => x.CreatedDate);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<MaterialRegionResponseDto>>();
    }
}
