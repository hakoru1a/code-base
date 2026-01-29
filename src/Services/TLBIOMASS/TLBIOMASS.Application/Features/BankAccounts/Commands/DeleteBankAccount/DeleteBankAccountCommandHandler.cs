using MediatR;
using TLBIOMASS.Domain.BankAccounts.Interfaces;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.DeleteBankAccount;

public class DeleteBankAccountCommandHandler : IRequestHandler<DeleteBankAccountCommand, bool>
{
    private readonly IBankAccountRepository _repository;

    public DeleteBankAccountCommandHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            return false;
        }

        await _repository.DeleteAsync(bankAccount, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
