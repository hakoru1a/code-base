using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Domain.Materials.ValueObjects;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial;

public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, bool>
{
    private readonly IMaterialRepository _repository;

    public UpdateMaterialCommandHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _repository.GetByIdAsync(request.Id);

        if (material == null)
        {
            throw new NotFoundException("Material", request.Id);
        }

        material.Update(
            new MaterialSpec(request.Name, request.Unit, request.Description, request.ProposedImpurityDeduction),
            request.Status);

        await _repository.UpdateAsync(material);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
