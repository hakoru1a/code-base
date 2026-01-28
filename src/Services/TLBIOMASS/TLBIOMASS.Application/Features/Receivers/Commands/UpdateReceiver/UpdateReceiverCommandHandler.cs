using MediatR;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Shared.Domain.ValueObjects;
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
            new ContactInfo(request.Phone, null, request.Address, request.Note),
            new BankInfo(request.BankAccount, request.BankName),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.IsActive);

        await _repository.UpdateAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
