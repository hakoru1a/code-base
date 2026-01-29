using MediatR;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;

public class CreateReceiverCommandHandler : IRequestHandler<CreateReceiverCommand, long>
{
    private readonly IReceiverRepository _repository;
    private readonly IBankAccountRepository _bankAccountRepository;

    public CreateReceiverCommandHandler(IReceiverRepository repository, IBankAccountRepository bankAccountRepository)
    {
        _repository = repository;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<long> Handle(CreateReceiverCommand request, CancellationToken cancellationToken)
    {
        var receiver = Receiver.Create(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.IsActive);

        await _repository.CreateAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        // Create polymorphic BankAccount if provided
        if (!string.IsNullOrWhiteSpace(request.BankAccount))
        {
            var bankAccount = BankAccount.Create(
                request.BankName ?? string.Empty,
                request.BankAccount,
                "Receiver",
                receiver.Id,
                true // Always default for legacy sync
            );
            await _bankAccountRepository.CreateAsync(bankAccount, cancellationToken);
            await _bankAccountRepository.SaveChangesAsync(cancellationToken);
        }

        return (long)receiver.Id;
    }
}
