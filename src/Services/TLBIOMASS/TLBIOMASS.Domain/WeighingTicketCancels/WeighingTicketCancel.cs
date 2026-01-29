using Contracts.Domain;
using TLBIOMASS.Domain.WeighingTicketCancels.Rules;

namespace TLBIOMASS.Domain.WeighingTicketCancels;

public class WeighingTicketCancel : EntityAuditBase<int>
{
    public int WeighingTicketId { get; private set; }
    public string? CancelReason { get; private set; }

    protected WeighingTicketCancel() { }

    private WeighingTicketCancel(int weighingTicketId, string? cancelReason)
    {
        WeighingTicketId = weighingTicketId;
        CancelReason = cancelReason;
    }

    public static WeighingTicketCancel Create(int weighingTicketId, string? cancelReason, bool isCancelled)
    {
        CheckRule(new WeighingTicketCancelMustBeUniqueRule(isCancelled, weighingTicketId));
        return new WeighingTicketCancel(weighingTicketId, cancelReason);
    }
}
