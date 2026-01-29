using Contracts.Domain;
using TLBIOMASS.Domain.Payments.Rules;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Payments.ValueObjects;

namespace TLBIOMASS.Domain.Payments;

public class PaymentDetail : EntityAuditBase<int>
{
    public int WeighingTicketId { get; private set; }
    public int? AgencyId { get; private set; }
    
    public PaymentInfo Info { get; private set; } = null!;
    public PaymentAmount PaymentAmount { get; private set; } = null!;
    public PaymentProcessStatus ProcessStatus { get; private set; } = null!;

    // Navigation properties
    public virtual WeighingTicket WeighingTicket { get; private set; } = null!;
    public virtual Agency Agency { get; private set; } = null!;

    protected PaymentDetail() { }

    private PaymentDetail(
        int weighingTicketId, 
        int? agencyId,
        PaymentInfo info,
        PaymentAmount paymentAmount,
        PaymentProcessStatus processStatus)
    {
        WeighingTicketId = weighingTicketId;
        AgencyId = agencyId;
        Info = info;
        PaymentAmount = paymentAmount;
        ProcessStatus = processStatus;
    }

    public static PaymentDetail Create(
        int weighingTicketId, 
        int? agencyId,
        PaymentInfo info,
        decimal amount, 
        decimal currentRemaining, // The remaining amount BEFORE this payment
        bool isPaid)
    {
        // Business Rule: Amount cannot exceed remaining
        CheckRule(new PaymentAmountCannotExceedRemainingRule(amount, currentRemaining));

        var newRemaining = currentRemaining - amount;
        var paymentAmount = new PaymentAmount(amount, newRemaining);
        var status = new PaymentProcessStatus(isPaid, false);

        return new PaymentDetail(
            weighingTicketId, 
            agencyId,
            info,
            paymentAmount,
            status);
    }

    public void UpdatePaymentStatus(bool isPaid)
    {
        CheckRule(new PaymentIsLockedRule(ProcessStatus.IsLocked));
            
        ProcessStatus = ProcessStatus with { IsPaid = isPaid };
    }

    public void Lock()
    {
        ProcessStatus = ProcessStatus with { IsLocked = true };
    }
}
