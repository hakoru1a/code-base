using MediatR;
using Shared.DTOs.BankAccount;

namespace TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountById;

public class GetBankAccountByIdQuery : IRequest<BankAccountResponseDto?>
{
    public int Id { get; set; }
}
