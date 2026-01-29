using Mapster;
using MediatR;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountsByOwner;

public class GetBankAccountsByOwnerQueryHandler : IRequestHandler<GetBankAccountsByOwnerQuery, List<BankAccountResponseDto>>
{
    private readonly IBankAccountRepository _repository;

    public GetBankAccountsByOwnerQueryHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<BankAccountResponseDto>> Handle(GetBankAccountsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var bankAccounts = await _repository.GetByOwnerAsync(request.OwnerType, request.OwnerId);
        return bankAccounts.Adapt<List<BankAccountResponseDto>>();
    }
}
