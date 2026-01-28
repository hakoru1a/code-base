using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Payments.Rules;

public class PaymentAmountCannotExceedRemainingRule : IBusinessRule
{
    private readonly decimal _amount;
    private readonly decimal _remaining;

    public PaymentAmountCannotExceedRemainingRule(decimal amount, decimal remaining)
    {
        _amount = amount;
        _remaining = remaining;
    }

    public bool IsBroken() => _amount > _remaining;

    public string Message => $"Payment amount ({_amount}) cannot exceed the remaining balance ({_remaining}).";

    public string Code => "Payment.AmountExceedsRemaining";
}
