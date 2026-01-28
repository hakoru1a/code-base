using MediatR;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;

public class UpdateReceiverCommandHandler : IRequestHandler<UpdateReceiverCommand, bool>
{
    private readonly IReceiverRepository _repository;

    public UpdateReceiverCommandHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateReceiverCommand request, CancellationToken cancellationToken)
    {
        var receiver = await _repository.GetByIdAsync(request.Id);

        if (receiver == null)
        {
            throw new NotFoundException("Receiver", request.Id);
        }

        receiver.Update(
            request.Name,
            request.Phone,
            request.BankAccount,
            request.BankName,
            request.IdentityNumber,
            request.IssuedDate,
            request.IssuedPlace,
            request.Address,
            request.IsDefault,
            request.IsActive,
            request.Note,
            request.DateOfBirth
        );

        await _repository.UpdateAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
