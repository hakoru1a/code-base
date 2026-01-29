using MediatR;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;

public class CreateReceiverCommandHandler : IRequestHandler<CreateReceiverCommand, long>
{
    private readonly IReceiverRepository _repository;

    public CreateReceiverCommandHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> Handle(CreateReceiverCommand request, CancellationToken cancellationToken)
    {
        var receiver = Receiver.Create(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.Status);

        // Create polymorphic BankAccount if provided and add to collection
        if (!string.IsNullOrWhiteSpace(request.BankAccount))
        {
            receiver.AddBankAccount(
                request.BankName ?? string.Empty,
                request.BankAccount,
                true // Always default for legacy sync
            );
        }

        await _repository.CreateWithoutSaveAsync(receiver, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)receiver.Id;
    }
}
