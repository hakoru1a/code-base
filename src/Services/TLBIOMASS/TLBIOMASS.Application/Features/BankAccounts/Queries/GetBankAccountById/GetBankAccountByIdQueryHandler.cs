using Mapster;
using MediatR;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts.Interfaces;

namespace TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountById;

public class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountResponseDto?>
{
    private readonly IBankAccountRepository _repository;

    public GetBankAccountByIdQueryHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<BankAccountResponseDto?> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            return null;
        }

        return bankAccount.Adapt<BankAccountResponseDto>();
    }
}
