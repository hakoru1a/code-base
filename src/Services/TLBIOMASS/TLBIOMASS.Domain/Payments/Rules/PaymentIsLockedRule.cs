using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Payments.Rules;

public class PaymentIsLockedRule : IBusinessRule
{
    private readonly bool _isLocked;

    public PaymentIsLockedRule(bool isLocked)
    {
        _isLocked = isLocked;
    }

    public bool IsBroken() => _isLocked;

    public string Message => "This payment record is locked and cannot be modified.";

    public string Code => "Payment.RecordLocked";
}
