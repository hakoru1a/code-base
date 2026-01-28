using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
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

        region.Update(
            request.RegionName,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.AreaHa,
            request.CertificateID,
            request.OwnerId
        );

        // Update Materials: Clear and Re-add for simplicity in this pattern
        region.ClearMaterials();
        
        if (request.RegionMaterials.Any())
        {
            var requestMaterialIds = request.RegionMaterials.Select(x => x.MaterialId).Distinct().ToList();
            
            // Validate existence
            var existingMaterialIds = await _materialRepository.FindAll()
                .Where(x => requestMaterialIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (existingMaterialIds.Count != requestMaterialIds.Count)
            {
                var missingIds = requestMaterialIds.Except(existingMaterialIds);
                throw new BadRequestException($"Materials with IDs [{string.Join(", ", missingIds)}] do not exist.");
            }

            foreach (var rmDto in request.RegionMaterials)
            {
                region.AddMaterial(rmDto.MaterialId, rmDto.AreaHa);
            }
        }

        await _repository.UpdateAsync(region);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
