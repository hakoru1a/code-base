using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.BankAccounts;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;

public class UpdateReceiverCommandHandler : IRequestHandler<UpdateReceiverCommand, bool>
{
    private readonly IReceiverRepository _repository;

    public UpdateReceiverCommandHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateReceiverCommand request, CancellationToken cancellationToken)
    {
        // Include BankAccounts and ENABLE TRACKING
        var receiver = await _repository.FindAll(trackChanges: true)
            .Include(x => x.BankAccounts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (receiver == null)
        {
            throw new NotFoundException("Receiver", request.Id);
        }

        // Update main entity (Directly modifying tracked entity)
        receiver.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.IsActive);

        // Smart Sync BankAccounts
        var requestIds = request.BankAccounts.Where(x => x.Id > 0).Select(x => x.Id).ToList();
        var existingIds = receiver.BankAccounts?.Select(x => x.Id).ToList() ?? new List<int>();

        // 1. Delete: IDs in DB but NOT in request
        var idsToDelete = existingIds.Except(requestIds).ToList();
        foreach (var id in idsToDelete)
        {
            var toRemove = receiver.BankAccounts.First(x => x.Id == id);
            receiver.BankAccounts.Remove(toRemove);
        }

        // 2. Update: IDs in both DB and request
        foreach (var dto in request.BankAccounts.Where(x => x.Id > 0))
        {
            var existing = receiver.BankAccounts.FirstOrDefault(x => x.Id == dto.Id);
            if (existing != null)
            {
                existing.Update(dto.BankName, dto.AccountNumber, dto.IsDefault);
            }
        }

        // 3. Create: IDs = 0
        foreach (var dto in request.BankAccounts.Where(x => x.Id == 0))
        {
            receiver.BankAccounts.Add(BankAccount.Create(
                dto.BankName,
                dto.AccountNumber,
                OwnerType.Receiver,
                receiver.Id,
                dto.IsDefault
            ));
        }

        // Ensure only one default bank account
        var defaults = receiver.BankAccounts.Where(x => x.IsDefault).ToList();
        if (defaults.Count > 1)
        {
            foreach (var acc in defaults.SkipLast(1))
            {
                acc.SetDefault(false);
            }
        }

        // EF Core tracks everything, one SaveChangesAsync = one Transaction
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
