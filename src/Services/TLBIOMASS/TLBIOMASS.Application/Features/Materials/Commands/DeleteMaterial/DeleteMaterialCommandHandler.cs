using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Materials.Commands.DeleteMaterial;

public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, bool>
{
    private readonly IMaterialRepository _repository;

    public DeleteMaterialCommandHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _repository.GetByIdAsync(request.Id);

        if (material == null)
        {
            throw new NotFoundException("Material", request.Id);
        }

        await _repository.DeleteAsync(material);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
