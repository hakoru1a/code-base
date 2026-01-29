using MediatR;
using Shared.DTOs.BankAccount;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommand : IRequest<bool>
{
    public int Id { get; set; }
    public BankAccountUpdateDto Data { get; set; } = null!;
}
