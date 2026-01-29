using MediatR;
using Shared.DTOs.BankAccount;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommand : IRequest<int>
{
    public BankAccountCreateDto Data { get; set; } = null!;
}
