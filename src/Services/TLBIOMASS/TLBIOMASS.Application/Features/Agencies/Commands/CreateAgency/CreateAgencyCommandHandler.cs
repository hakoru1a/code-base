using MediatR;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Application.Features.Agencies.Commands.CreateAgency
{
    public class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, long>
    {
        private readonly IAgencyRepository _repository;

        public CreateAgencyCommandHandler(IAgencyRepository repository)
        {
            _repository = repository;
        }

        public async Task<long> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
        {
            var agency = Agency.Create(
                request.Name,
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
                request.Status);

            // Create polymorphic BankAccount if provided and add to collection
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                agency.AddBankAccount(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    true
                );
            }

            await _repository.CreateWithoutSaveAsync(agency, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)agency.Id;
        }
    }
}
