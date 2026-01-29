using MediatR;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, bool>
{
    private readonly IBankAccountRepository _repository;

    public UpdateBankAccountCommandHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            return false;
        }

        // If setting as default, unset previous default
        if (request.Data.IsDefault && !bankAccount.IsDefault)
        {
            var defaultAccount = await _repository.GetDefaultByOwnerAsync(bankAccount.OwnerType, bankAccount.OwnerId);
            if (defaultAccount != null && defaultAccount.Id != bankAccount.Id)
            {
                defaultAccount.SetDefault(false);
                await _repository.UpdateAsync(defaultAccount, cancellationToken);
            }
        }

        bankAccount.Update(request.Data.BankName, request.Data.AccountNumber, request.Data.IsDefault);
        await _repository.UpdateAsync(bankAccount, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
