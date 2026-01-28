using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.Domain.ValueObjects;
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
        var landowner = await _repository.GetByIdAsync(request.Id);

        if (landowner == null)
        {
            throw new NotFoundException("Landowner", request.Id);
        }

        landowner.Update(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, null),
            new BankInfo(request.BankAccount, request.BankName),
            new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
            request.IsActive);

        await _repository.UpdateAsync(landowner);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
