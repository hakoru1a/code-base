using MediatR;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.Agencies.Events;

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
                request.Phone,
                request.Email,
                request.Address,
                request.BankAccount,
                request.BankName,
                request.IdentityCard,
                request.IssuePlace,
                request.IssueDate,
                request.IsActive
            );

            //agency.AddDomainEvent(new AgencyCreatedEvent(agency.Id, agency.Name));

            await _repository.CreateAsync(agency);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)agency.Id;
        }
    }
}
