using MediatR;
using Shared.DTOs.BankAccount;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountsByOwner;

public class GetBankAccountsByOwnerQuery : IRequest<List<BankAccountResponseDto>>
{
    public OwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }
}
