using MediatR;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.SetDefaultBankAccount;

public class SetDefaultBankAccountCommand : IRequest<bool>
{
    public int Id { get; set; }
}
