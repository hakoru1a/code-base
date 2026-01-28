using MediatR;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Receivers.Interfaces;

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
            request.Phone,
            request.BankAccount,
            request.BankName,
            request.IdentityNumber,
            request.IssuedDate,
            request.IssuedPlace,
            request.Address,
            request.IsDefault,
            request.IsActive,
            request.Note,
            request.DateOfBirth
        );

        await _repository.CreateAsync(receiver);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)receiver.Id;
    }
}
