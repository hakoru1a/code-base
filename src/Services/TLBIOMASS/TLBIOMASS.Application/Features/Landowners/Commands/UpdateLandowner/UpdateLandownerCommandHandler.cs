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

        // Update main entity
        landowner.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, null),
            new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
            request.IsActive);

        // Explicit Sync BankAccounts using Domain Logic
        foreach (var bankAccountDto in request.BankAccounts)
        {
            landowner.ApplyBankAccountChange(bankAccountDto);
        }
       
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
