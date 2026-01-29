using MediatR;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Events.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Commands.CreateAgency
{
    public class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, long>
    {
        private readonly IAgencyRepository _repository;
        private readonly IBankAccountRepository _bankAccountRepository;

        public CreateAgencyCommandHandler(IAgencyRepository repository, IBankAccountRepository bankAccountRepository)
        {
            _repository = repository;
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<long> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
        {
            var agency = Agency.Create(
                request.Name,
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
                request.IsActive);

            //agency.AddDomainEvent(new AgencyCreatedEvent(agency.Id, agency.Name));

            await _repository.CreateAsync(agency);
            await _repository.SaveChangesAsync(cancellationToken);

            // Create polymorphic BankAccount if provided
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                var bankAccount = BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Agency",
                    agency.Id,
                    true // Set as default for new agency
                );
                await _bankAccountRepository.CreateAsync(bankAccount, cancellationToken);
                await _bankAccountRepository.SaveChangesAsync(cancellationToken);
            }

            return (long)agency.Id;
        }
    }
}
