using MediatR;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.MaterialRegions.Commands.DeleteMaterialRegion;

public class DeleteMaterialRegionCommandHandler : IRequestHandler<DeleteMaterialRegionCommand, bool>
{
    private readonly IMaterialRegionRepository _repository;

    public DeleteMaterialRegionCommandHandler(IMaterialRegionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteMaterialRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _repository.GetByIdAsync(request.Id);

        if (region == null)
        {
            throw new NotFoundException("MaterialRegion", request.Id);
        }

        await _repository.DeleteAsync(region);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
