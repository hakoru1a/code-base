using MediatR;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Domain.Materials.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Events.MaterialRegion;

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
        var region = MaterialRegion.Create(
            request.RegionName,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.AreaHa,
            request.CertificateID,
            request.OwnerId
        );

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

        //region.AddDomainEvent(new MaterialRegionCreatedEvent(region.Id, region.RegionName));

        await _repository.CreateAsync(region);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)region.Id;
    }
}
