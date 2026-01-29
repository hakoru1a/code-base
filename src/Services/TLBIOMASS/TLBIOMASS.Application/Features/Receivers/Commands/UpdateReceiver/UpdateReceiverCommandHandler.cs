using MediatR;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Shared.Domain.ValueObjects;
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

        // Update main entity
        receiver.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.Status);

        // Explicit Sync BankAccounts using Domain Logic (Composition)
        receiver.SyncBankAccounts(request.BankAccounts);

        // EF Core tracks everything, one SaveChangesAsync = one Transaction
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
