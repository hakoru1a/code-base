using MediatR;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Receivers.Commands.DeleteReceiver;

public class DeleteReceiverCommandHandler : IRequestHandler<DeleteReceiverCommand, bool>
{
    private readonly IReceiverRepository _repository;

    public DeleteReceiverCommandHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteReceiverCommand request, CancellationToken cancellationToken)
    {
        var receiver = await _repository.GetByIdAsync(request.Id);

        if (receiver == null)
        {
            throw new NotFoundException("Receiver", request.Id);
        }

        await _repository.DeleteAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
