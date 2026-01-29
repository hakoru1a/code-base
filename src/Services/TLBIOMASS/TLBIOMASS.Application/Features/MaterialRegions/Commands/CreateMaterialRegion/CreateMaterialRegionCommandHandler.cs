using MediatR;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;
using TLBIOMASS.Domain.Materials.Interfaces;
using Microsoft.EntityFrameworkCore;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.MaterialRegions.Commands.CreateMaterialRegion;

public class CreateMaterialRegionCommandHandler : IRequestHandler<CreateMaterialRegionCommand, long>
{
    private readonly IMaterialRegionRepository _repository;
    private readonly IMaterialRepository _materialRepository;

    public CreateMaterialRegionCommandHandler(IMaterialRegionRepository repository, IMaterialRepository materialRepository)
    {
        _repository = repository;
        _materialRepository = materialRepository;
    }

    public async Task<long> Handle(CreateMaterialRegionCommand request, CancellationToken cancellationToken)
    {
        var detail = new RegionDetail(request.RegionName, request.Address, request.Latitude, request.Longitude, request.AreaHa, request.CertificateID);
        var region = MaterialRegion.Create(detail, request.OwnerId);

        if (request.RegionMaterials.Any())
        {
            var requestMaterialIds = request.RegionMaterials.Select(x => x.MaterialId).Distinct().ToList();
            
            // Validate that all materials exist
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

        await _repository.CreateAsync(region);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)region.Id;
    }
}
