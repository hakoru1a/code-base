using MediatR;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.SetDefaultBankAccount;

public class SetDefaultBankAccountCommandHandler : IRequestHandler<SetDefaultBankAccountCommand, bool>
{
    private readonly IBankAccountRepository _repository;

    public SetDefaultBankAccountCommandHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(SetDefaultBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            return false;
        }

        if (!bankAccount.IsDefault)
        {
            var defaultAccount = await _repository.GetDefaultByOwnerAsync(bankAccount.OwnerType, bankAccount.OwnerId);
            if (defaultAccount != null)
            {
                defaultAccount.SetDefault(false);
                await _repository.UpdateAsync(defaultAccount, cancellationToken);
            }

            bankAccount.SetDefault(true);
            await _repository.UpdateAsync(bankAccount, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
