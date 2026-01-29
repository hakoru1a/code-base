using MediatR;

namespace TLBIOMASS.Application.Features.BankAccounts.Commands.DeleteBankAccount;

public class DeleteBankAccountCommand : IRequest<bool>
{
    public int Id { get; set; }
}
