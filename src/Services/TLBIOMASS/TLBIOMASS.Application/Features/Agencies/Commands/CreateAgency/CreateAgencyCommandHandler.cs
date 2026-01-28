using MediatR;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Events.Agency;

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
                new ContactInfo(request.Phone, request.Email, request.Address,null),
                new BankInfo(request.BankAccount, request.BankName),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
                request.IsActive);

            //agency.AddDomainEvent(new AgencyCreatedEvent(agency.Id, agency.Name));

            await _repository.CreateAsync(agency);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)agency.Id;
        }
    }
}
