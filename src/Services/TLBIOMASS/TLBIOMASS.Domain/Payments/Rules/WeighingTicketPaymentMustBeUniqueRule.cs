using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Payments.Rules;

public class WeighingTicketPaymentMustBeUniqueRule : IBusinessRule
{
    private readonly bool _alreadyExists;

    public WeighingTicketPaymentMustBeUniqueRule(bool alreadyExists)
    {
        _alreadyExists = alreadyExists;
    }

    public bool IsBroken() => _alreadyExists;

    public string Message => "Payment configuration for this weighing ticket already exists.";

    public string Code => "Payment.ConfigAlreadyExists";
}
