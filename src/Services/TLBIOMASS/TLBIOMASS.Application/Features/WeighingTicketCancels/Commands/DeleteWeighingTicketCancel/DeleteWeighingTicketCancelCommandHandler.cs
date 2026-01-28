using MediatR;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.DeleteWeighingTicketCancel;

public class DeleteWeighingTicketCancelCommandHandler : IRequestHandler<DeleteWeighingTicketCancelCommand, bool>
{
    private readonly IWeighingTicketCancelRepository _repository;

    public DeleteWeighingTicketCancelCommandHandler(IWeighingTicketCancelRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteWeighingTicketCancelCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            throw new NotFoundException("WeighingTicketCancel", request.Id);

        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
