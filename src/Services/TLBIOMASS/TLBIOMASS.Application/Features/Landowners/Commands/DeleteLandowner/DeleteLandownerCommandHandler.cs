using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Landowners.Commands.DeleteLandowner;

public class DeleteLandownerCommandHandler : IRequestHandler<DeleteLandownerCommand, bool>
{
    private readonly ILandownerRepository _repository;

    public DeleteLandownerCommandHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteLandownerCommand request, CancellationToken cancellationToken)
    {
        var landowner = await _repository.GetByIdAsync(request.Id);

        if (landowner == null)
        {
            throw new NotFoundException("Landowner", request.Id);
        }

        await _repository.DeleteAsync(landowner);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
