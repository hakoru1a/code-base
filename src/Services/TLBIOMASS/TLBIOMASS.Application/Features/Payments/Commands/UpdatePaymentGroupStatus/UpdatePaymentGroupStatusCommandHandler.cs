using MediatR;
using TLBIOMASS.Domain.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.Payments.Commands.UpdatePaymentGroupStatus;

public class UpdatePaymentGroupStatusCommandHandler : IRequestHandler<UpdatePaymentGroupStatusCommand, bool>
{
    private readonly IPaymentDetailRepository _repository;

    public UpdatePaymentGroupStatusCommandHandler(IPaymentDetailRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdatePaymentGroupStatusCommand request, CancellationToken cancellationToken)
    {
        var details = await _repository.FindAll(trackChanges: true)
            .Where(x => x.Info.PaymentCode == request.PaymentCode)
            .ToListAsync(cancellationToken);

        if (!details.Any()) return false;

        foreach (var detail in details)
        {
            detail.UpdatePaymentStatus(request.IsPaid);
            if (request.ShouldLock)
            {
                detail.Lock();
            }
        }

        await _repository.UpdateListAsync(details);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
