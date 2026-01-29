using MediatR;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;

public class UpdateReceiverCommandHandler : IRequestHandler<UpdateReceiverCommand, bool>
{
    private readonly IReceiverRepository _repository;
    private readonly IBankAccountRepository _bankAccountRepository;

    public UpdateReceiverCommandHandler(IReceiverRepository repository, IBankAccountRepository bankAccountRepository)
    {
        _repository = repository;
        _bankAccountRepository = bankAccountRepository;
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
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.IsActive);


        await _repository.UpdateAsync(receiver);

        // Sync polymorphic BankAccount
        if (!string.IsNullOrWhiteSpace(request.BankAccount))
        {
            var defaultAccount = await _bankAccountRepository.GetDefaultByOwnerAsync("Receiver", receiver.Id);
            if (defaultAccount != null)
            {
                defaultAccount.Update(request.BankName ?? string.Empty, request.BankAccount, true); // Always default
                await _bankAccountRepository.UpdateAsync(defaultAccount, cancellationToken);
            }
            else
            {
                var newAccount = BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Receiver",
                    receiver.Id,
                    true // Always default
                );
                await _bankAccountRepository.CreateAsync(newAccount, cancellationToken);
            }
            await _bankAccountRepository.SaveChangesAsync(cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
