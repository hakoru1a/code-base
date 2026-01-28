using MediatR;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Domain.Materials.ValueObjects;
using Shared.Events.Material;

namespace TLBIOMASS.Application.Features.Materials.Commands.CreateMaterial;

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, long>
{
    private readonly IMaterialRepository _repository;

    public CreateMaterialCommandHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = Material.Create(
            new MaterialSpec(request.Name, request.Unit, request.Description, request.ProposedImpurityDeduction),
            request.IsActive);

        //material.AddDomainEvent(new MaterialCreatedEvent(material.Id, material.Name));

        await _repository.CreateAsync(material);
        await _repository.SaveChangesAsync(cancellationToken);

        return material.Id;
    }
}
