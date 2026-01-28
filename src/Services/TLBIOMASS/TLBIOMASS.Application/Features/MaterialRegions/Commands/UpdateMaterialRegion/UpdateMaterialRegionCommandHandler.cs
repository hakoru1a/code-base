using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;
using TLBIOMASS.Domain.Materials.Interfaces;
using Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.MaterialRegions.Commands.UpdateMaterialRegion;

public class UpdateMaterialRegionCommandHandler : IRequestHandler<UpdateMaterialRegionCommand, bool>
{
    private readonly IMaterialRegionRepository _repository;
    private readonly IMaterialRepository _materialRepository;

    public UpdateMaterialRegionCommandHandler(IMaterialRegionRepository repository, IMaterialRepository materialRepository)
    {
        _repository = repository;
        _materialRepository = materialRepository;
    }

    public async Task<bool> Handle(UpdateMaterialRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _repository.FindAll()
            .Include(x => x.RegionMaterials)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (region == null)
        {
            throw new NotFoundException("MaterialRegion", request.Id);
        }

        var detail = new RegionDetail(request.RegionName, request.Address, request.Latitude, request.Longitude, request.AreaHa, request.CertificateID);
        region.Update(detail, request.OwnerId);

        // Update Materials: Clear and Re-add for simplicity in this pattern
        region.ClearMaterials();
        
        if (request.RegionMaterials.Any())
        {
            var materialIds = request.RegionMaterials.Select(x => x.MaterialId).ToList();
            var materials = await _materialRepository.FindAll()
                .Where(x => materialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var rmDto in request.RegionMaterials)
            {
                var material = materials.FirstOrDefault(x => x.Id == rmDto.MaterialId);
                if (material != null)
                {
                    region.AddMaterial(material, rmDto.AreaHa);
                }
            }
        }

        await _repository.UpdateAsync(region);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
