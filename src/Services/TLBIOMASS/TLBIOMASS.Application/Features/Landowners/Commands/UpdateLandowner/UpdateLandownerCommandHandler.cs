using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Landowners.Commands.UpdateLandowner;

public class UpdateLandownerCommandHandler : IRequestHandler<UpdateLandownerCommand, bool>
{
    private readonly ILandownerRepository _repository;
    private readonly IBankAccountRepository _bankAccountRepository;

    public UpdateLandownerCommandHandler(ILandownerRepository repository, IBankAccountRepository bankAccountRepository)
    {
        _repository = repository;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<bool> Handle(UpdateLandownerCommand request, CancellationToken cancellationToken)
    {
        var landowner = await _repository.GetByIdAsync(request.Id);

        if (landowner == null)
        {
            throw new NotFoundException("Landowner", request.Id);
        }

        landowner.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, null),
            new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
            request.IsActive);


        await _repository.UpdateAsync(landowner);

        // Sync polymorphic BankAccount
        if (!string.IsNullOrWhiteSpace(request.BankAccount))
        {
            var defaultAccount = await _bankAccountRepository.GetDefaultByOwnerAsync("Landowner", landowner.Id);
            if (defaultAccount != null)
            {
                defaultAccount.Update(request.BankName ?? string.Empty, request.BankAccount, true);
                await _bankAccountRepository.UpdateAsync(defaultAccount, cancellationToken);
            }
            else
            {
                var newAccount = BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Landowner",
                    landowner.Id,
                    true
                );
                await _bankAccountRepository.CreateAsync(newAccount, cancellationToken);
            }
            await _bankAccountRepository.SaveChangesAsync(cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
