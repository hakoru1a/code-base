using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.Domain.ValueObjects;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Agencies.Commands.UpdateAgency
{
    public class UpdateAgencyCommandHandler : IRequestHandler<UpdateAgencyCommand, bool>
    {
        private readonly IAgencyRepository _repository;

        public UpdateAgencyCommandHandler(IAgencyRepository repository)
        {
            _repository = repository;
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
                new ContactInfo(request.Phone, request.Email, request.Address),
                new BankInfo(request.BankAccount, request.BankName),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate),
                request.IsActive);

            await _repository.UpdateAsync(agency);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
