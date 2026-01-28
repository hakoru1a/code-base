using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Payments.Rules;

public class PriceCannotBeChangedAfterPaymentRule : IBusinessRule
{
    private readonly bool _hasPayments;

    public PriceCannotBeChangedAfterPaymentRule(bool hasPayments)
    {
        _hasPayments = hasPayments;
    }

    public bool IsBroken() => _hasPayments;

    public string Message => "Price cannot be changed after the first payment has been made.";

    public string Code => "Payment.PriceLockedAfterPayment";
}
