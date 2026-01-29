using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
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
            // Include BankAccounts and ENABLE TRACKING
            var agency = await _repository.FindAll(trackChanges: true)
                .Include(x => x.BankAccounts)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (agency == null)
            {
                throw new NotFoundException("Agency", request.Id);
            }

            // Update main entity (Directly modifying tracked entity)
            agency.Update(
                request.Name,
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCard, request.IssuePlace, request.IssueDate, null),
                request.IsActive);

            // Smart Sync BankAccounts (Modifying tracked collection)
            var bankAccounts = agency.BankAccounts ?? throw new InvalidOperationException(
                "Agency BankAccounts collection must be loaded. Use Include(x => x.BankAccounts).");

            var requestIds = request.BankAccounts.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var existingIds = bankAccounts.Select(x => x.Id).ToList();

            // 1. Delete: IDs in DB but NOT in request
            var idsToDelete = existingIds.Except(requestIds).ToList();
            foreach (var id in idsToDelete)
            {
                var toRemove = bankAccounts.First(x => x.Id == id);
                bankAccounts.Remove(toRemove);
            }

            // 2. Update: IDs in both DB and request
            foreach (var dto in request.BankAccounts.Where(x => x.Id > 0))
            {
                var existing = bankAccounts.FirstOrDefault(x => x.Id == dto.Id);
                if (existing != null)
                {
                    existing.Update(dto.BankName, dto.AccountNumber, dto.IsDefault);
                }
            }

            // 3. Create: IDs = 0
            foreach (var dto in request.BankAccounts.Where(x => x.Id == 0))
            {
                bankAccounts.Add(BankAccount.Create(
                    dto.BankName,
                    dto.AccountNumber,
                    "Agency",
                    agency.Id,
                    dto.IsDefault
                ));
            }

            // Ensure only one default
            var defaults = bankAccounts.Where(x => x.IsDefault).ToList();
            if (defaults.Count > 1)
            {
                foreach (var acc in defaults.SkipLast(1))
                {
                    acc.SetDefault(false);
                }
            }

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
