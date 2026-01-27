using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById;

public class GetMaterialRegionByIdQueryHandler : IRequestHandler<GetMaterialRegionByIdQuery, MaterialRegionResponseDto>
{
    private readonly IMaterialRegionRepository _repository;

    public GetMaterialRegionByIdQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<MaterialRegionResponseDto> Handle(GetMaterialRegionByIdQuery request, CancellationToken cancellationToken)
    {
        var region = await _repository.FindAll()
            .Include(x => x.RegionMaterials)
            .ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
        return region?.Adapt<MaterialRegionResponseDto>();
    }
}
