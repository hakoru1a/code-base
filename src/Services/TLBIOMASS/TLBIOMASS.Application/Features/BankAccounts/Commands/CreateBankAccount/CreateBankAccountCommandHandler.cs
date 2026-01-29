using MediatR;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, int>
{
    private readonly IBankAccountRepository _repository;

    public CreateBankAccountCommandHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = BankAccount.Create(
            request.Data.BankName,
            request.Data.AccountNumber,
            request.Data.OwnerType,
            request.Data.OwnerId,
            request.Data.IsDefault
        );

        // If this is the default bank account, we should unset other default accounts for this owner
        if (request.Data.IsDefault)
        {
            var defaultAccount = await _repository.GetDefaultByOwnerAsync(request.Data.OwnerType, request.Data.OwnerId);
            if (defaultAccount != null)
            {
                defaultAccount.SetDefault(false);
                await _repository.UpdateAsync(defaultAccount, cancellationToken);
            }
        }

        await _repository.CreateAsync(bankAccount, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return bankAccount.Id;
    }
}
