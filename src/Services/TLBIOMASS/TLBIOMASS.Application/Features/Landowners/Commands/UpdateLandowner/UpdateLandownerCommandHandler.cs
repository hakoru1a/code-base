using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Landowners.Commands.UpdateLandowner;

public class UpdateLandownerCommandHandler : IRequestHandler<UpdateLandownerCommand, bool>
{
    private readonly ILandownerRepository _repository;

    public UpdateLandownerCommandHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateLandownerCommand request, CancellationToken cancellationToken)
    {
        // Include BankAccounts and ENABLE TRACKING
        var landowner = await _repository.FindAll(trackChanges: true)
            .Include(x => x.BankAccounts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (landowner == null)
        {
            throw new NotFoundException("Landowner", request.Id);
        }

        // Update main entity (Directly modifying tracked entity)
        landowner.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, null),
            new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
            request.IsActive);

        // Smart Sync BankAccounts
        var requestIds = request.BankAccounts.Where(x => x.Id > 0).Select(x => x.Id).ToList();
        var existingIds = landowner.BankAccounts?.Select(x => x.Id).ToList() ?? new List<int>();

        // 1. Delete: IDs in DB but NOT in request
        var idsToDelete = existingIds.Except(requestIds).ToList();
        foreach (var id in idsToDelete)
        {
            var toRemove = landowner.BankAccounts.First(x => x.Id == id);
            landowner.BankAccounts.Remove(toRemove);
        }

        // 2. Update: IDs in both DB and request
        foreach (var dto in request.BankAccounts.Where(x => x.Id > 0))
        {
            var existing = landowner.BankAccounts.FirstOrDefault(x => x.Id == dto.Id);
            if (existing != null)
            {
                existing.Update(dto.BankName, dto.AccountNumber, dto.IsDefault);
            }
        }

        // 3. Create: IDs = 0
        foreach (var dto in request.BankAccounts.Where(x => x.Id == 0))
        {
            landowner.BankAccounts.Add(BankAccount.Create(
                dto.BankName,
                dto.AccountNumber,
                OwnerType.Landowner,
                landowner.Id,
                dto.IsDefault
            ));
        }

        // Ensure only one default
        var defaults = landowner.BankAccounts.Where(x => x.IsDefault).ToList();
        if (defaults.Count > 1)
        {
            // Keep only the last one as default
            foreach (var acc in defaults.SkipLast(1))
            {
                acc.SetDefault(false);
            }
        }

       
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
