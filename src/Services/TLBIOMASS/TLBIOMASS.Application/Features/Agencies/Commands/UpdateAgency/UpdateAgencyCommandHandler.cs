using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
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
            // Include BankAccounts and ENABLE TRACKING
            var agency = await _repository.FindAll(trackChanges: true)
                .Include(x => x.BankAccounts)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (agency == null)
        {
            throw new NotFoundException("Agency", request.Id);
        }

        // Update main entity
        agency.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, null),
            new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
            request.IsActive);

        // Explicit Sync BankAccounts using Domain Logic
        foreach (var bankAccountDto in request.BankAccounts)
        {
            agency.ApplyBankAccountChange(bankAccountDto);
        }

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
