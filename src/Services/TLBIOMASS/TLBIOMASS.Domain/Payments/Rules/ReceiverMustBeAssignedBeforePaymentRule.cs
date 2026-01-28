using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Payments.Rules;

public class ReceiverMustBeAssignedBeforePaymentRule : IBusinessRule
{
    private readonly int? _receiverId;

    public ReceiverMustBeAssignedBeforePaymentRule(int? receiverId)
    {
        _receiverId = receiverId;
    }

    public bool IsBroken() => !_receiverId.HasValue;

    public string Message => "A receiver must be assigned to the weighing ticket before making a payment.";

    public string Code => "Payment.ReceiverNotAssigned";
}
