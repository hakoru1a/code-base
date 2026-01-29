using MediatR;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;

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

            //landowner.AddDomainEvent(new LandownerCreatedEvent(landowner.Id, landowner.Name));

            await _repository.CreateAsync(landowner);
            await _repository.SaveChangesAsync(cancellationToken);

            // Create polymorphic BankAccount if provided
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                var bankAccount = BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Landowner",
                    landowner.Id,
                    true
                );
                await _bankAccountRepository.CreateAsync(bankAccount, cancellationToken);
                await _bankAccountRepository.SaveChangesAsync(cancellationToken);
            }

            return (long)landowner.Id;
        }
    }
}
