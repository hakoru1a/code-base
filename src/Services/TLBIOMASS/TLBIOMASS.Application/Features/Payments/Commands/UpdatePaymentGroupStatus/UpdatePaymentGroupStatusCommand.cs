using MediatR;

namespace TLBIOMASS.Application.Features.Payments.Commands.UpdatePaymentGroupStatus;

public class UpdatePaymentGroupStatusCommand : IRequest<bool>
{
    public string PaymentCode { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool ShouldLock { get; set; }
}
