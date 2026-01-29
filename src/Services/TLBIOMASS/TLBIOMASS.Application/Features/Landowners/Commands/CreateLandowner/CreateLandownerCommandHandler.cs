using MediatR;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;

namespace TLBIOMASS.Application.Features.Landowners.Commands.CreateLandowner
{
    public class CreateLandownerCommandHandler : IRequestHandler<CreateLandownerCommand, long>
    {
        private readonly ILandownerRepository _repository;
        private readonly IBankAccountRepository _bankAccountRepository;

        public CreateLandownerCommandHandler(ILandownerRepository repository, IBankAccountRepository bankAccountRepository)
        {
            _repository = repository;
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<long> Handle(CreateLandownerCommand request, CancellationToken cancellationToken)
        {
            var landowner = Landowner.Create(
                request.Name,
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
                request.IsActive);

            // Create polymorphic BankAccount if provided and add to collection
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                landowner.BankAccounts.Add(BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Landowner",
                    0, // EF will fill this after save
                    true
                ));
            }

            await _repository.CreateWithoutSaveAsync(landowner, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)landowner.Id;
        }
    }
}
