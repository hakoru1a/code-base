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

            // Create polymorphic BankAccount if provided and add to collection
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                agency.BankAccounts.Add(BankAccount.Create(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    "Agency",
                    0, // EF Core will map this after save
                    true
                ));
            }

            await _repository.CreateWithoutSaveAsync(agency, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)agency.Id;
        }
    }
}
