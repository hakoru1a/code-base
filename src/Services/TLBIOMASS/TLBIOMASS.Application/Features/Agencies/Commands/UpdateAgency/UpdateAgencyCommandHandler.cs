using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using Shared.Domain.ValueObjects;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Agencies.Commands.UpdateAgency
{
    public class UpdateAgencyCommandHandler : IRequestHandler<UpdateAgencyCommand, bool>
    {
        private readonly IAgencyRepository _repository;
        private readonly IBankAccountRepository _bankAccountRepository;

        public UpdateAgencyCommandHandler(IAgencyRepository repository, IBankAccountRepository bankAccountRepository)
        {
            _repository = repository;
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<bool> Handle(UpdateAgencyCommand request, CancellationToken cancellationToken)
        {
            var agency = await _repository.GetByIdAsync(request.Id);

            if (agency == null)
            {
                throw new NotFoundException("Agency", request.Id);
            }

            agency.Update(
                request.Name,
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
                request.IsActive);


            await _repository.UpdateAsync(agency);
            
            // Sync polymorphic BankAccount
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                var defaultAccount = await _bankAccountRepository.GetDefaultByOwnerAsync("Agency", agency.Id);
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
                        "Agency",
                        agency.Id,
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
}
