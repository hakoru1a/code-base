using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Shared.DTOs.MaterialRegion;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById;

public class GetMaterialRegionByIdQueryHandler : IRequestHandler<GetMaterialRegionByIdQuery, MaterialRegionResponseDto?>
{
    private readonly IMaterialRegionRepository _repository;

    public GetMaterialRegionByIdQueryHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<MaterialRegionResponseDto?> Handle(GetMaterialRegionByIdQuery request, CancellationToken cancellationToken)
    {
        var region = await _repository.FindAll()
            .Include(x => x.Owner)
            .Include(x => x.RegionMaterials)
            .ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
        if (region == null)
        {
            throw new NotFoundException("MaterialRegion", request.Id);
        }

        return region.Adapt<MaterialRegionResponseDto>();
    }
}
