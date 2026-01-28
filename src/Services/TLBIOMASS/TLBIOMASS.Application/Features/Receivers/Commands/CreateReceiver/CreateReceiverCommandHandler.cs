using MediatR;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;

public class CreateReceiverCommandHandler : IRequestHandler<CreateReceiverCommand, long>
{
    private readonly IReceiverRepository _repository;

    public CreateReceiverCommandHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> Handle(CreateReceiverCommand request, CancellationToken cancellationToken)
    {
        var receiver = Receiver.Create(
            request.Name,
            new ContactInfo(request.Phone, null, request.Address, request.Note),
            new BankInfo(request.BankAccount, request.BankName),
            new IdentityInfo(request.IdentityNumber, request.IssuedPlace, request.IssuedDate, request.DateOfBirth),
            request.IsDefault,
            request.IsActive);

        await _repository.CreateAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)receiver.Id;
    }
}
