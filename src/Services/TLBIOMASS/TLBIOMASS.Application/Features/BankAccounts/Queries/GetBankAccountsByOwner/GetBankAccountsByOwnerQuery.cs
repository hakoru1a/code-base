using MediatR;
using Shared.DTOs.BankAccount;

namespace TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountsByOwner;

public class GetBankAccountsByOwnerQuery : IRequest<List<BankAccountResponseDto>>
{
    public string OwnerType { get; set; } = string.Empty;
    public int OwnerId { get; set; }
}
