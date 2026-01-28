using Contracts.Domain;
using TLBIOMASS.Domain.Payments.Rules;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.Agencies;

namespace TLBIOMASS.Domain.Payments;

public class PaymentDetail : EntityAuditBase<int>
{
    public int WeighingTicketId { get; private set; }
    public string PaymentCode { get; private set; } = string.Empty;
    public DateTime PaymentDate { get; private set; }
    public int? AgencyId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public string? Note { get; private set; }
    public bool IsPaid { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTime? CustomerPaymentDate { get; private set; }

    // Navigation properties
    public virtual WeighingTicket WeighingTicket { get; private set; } = null!;
    public virtual Agency Agency { get; private set; } = null!;

    protected PaymentDetail() { }

    private PaymentDetail(
        int weighingTicketId, 
        string paymentCode,
        DateTime paymentDate,
        int? agencyId,
        decimal amount, 
        decimal remainingAmount, 
        string? note, 
        bool isPaid, 
        DateTime? customerPaymentDate)
    {
        WeighingTicketId = weighingTicketId;
        PaymentCode = paymentCode;
        PaymentDate = paymentDate;
        AgencyId = agencyId;
        Amount = amount;
        RemainingAmount = remainingAmount;
        Note = note;
        IsPaid = isPaid;
        IsLocked = false;
        CustomerPaymentDate = customerPaymentDate;
    }

    public static PaymentDetail Create(
        int weighingTicketId, 
        string paymentCode,
        DateTime paymentDate,
        int? agencyId,
        decimal amount, 
        decimal currentRemaining, // The remaining amount BEFORE this payment
        string? note, 
        bool isPaid, 
        DateTime? customerPaymentDate)
    {
        // Business Rule: Amount cannot exceed remaining
        CheckRule(new PaymentAmountCannotExceedRemainingRule(amount, currentRemaining));

        var newRemaining = currentRemaining - amount;

        return new PaymentDetail(
            weighingTicketId, 
            paymentCode,
            paymentDate,
            agencyId,
            amount, 
            newRemaining, 
            note, 
            isPaid, 
            customerPaymentDate);
    }

    // Explicitly NO Update or Delete methods for Amount/RemainingAmount to enforce immutability.
    
    public void UpdatePaymentStatus(bool isPaid)
    {
        CheckRule(new PaymentIsLockedRule(IsLocked));
            
        IsPaid = isPaid;
    }

    public void Lock()
    {
        IsLocked = true;
    }
}
