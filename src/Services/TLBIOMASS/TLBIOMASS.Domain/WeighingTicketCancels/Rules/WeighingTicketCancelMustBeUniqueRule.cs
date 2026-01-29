using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.WeighingTicketCancels.Rules;

public class WeighingTicketCancelMustBeUniqueRule : IBusinessRule
{
    private readonly bool _exists;
    private readonly int _ticketId;

    public WeighingTicketCancelMustBeUniqueRule(bool exists, int ticketId)
    {
        _exists = exists;
        _ticketId = ticketId;
    }

    public bool IsBroken() => _exists;

    public string Message => $"WeighingTicket with Id '{_ticketId}' is already cancelled.";

    public string Code => "WeighingTicketCancel.WeighingTicketCancelMustBeUnique";
}
