using Contracts.Domain;
using TLBIOMASS.Domain.Payments.Rules;

namespace TLBIOMASS.Domain.Payments;

public class WeighingTicketPayment : EntityAuditBase<int>
{
    public int WeighingTicketId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPayableAmount { get; private set; }
    public string? Note { get; private set; }

    protected WeighingTicketPayment() { }

    private WeighingTicketPayment(int weighingTicketId, decimal unitPrice, decimal totalPayableAmount, string? note)
    {
        WeighingTicketId = weighingTicketId;
        UnitPrice = unitPrice;
        TotalPayableAmount = totalPayableAmount;
        Note = note;
        CreatedDate = DateTime.UtcNow;
    }

    public static WeighingTicketPayment Create(int weighingTicketId, decimal unitPrice, decimal totalPayableAmount, string? note, bool alreadyExists)
    {
        CheckRule(new WeighingTicketPaymentMustBeUniqueRule(alreadyExists));
        return new WeighingTicketPayment(weighingTicketId, unitPrice, totalPayableAmount, note);
    }

    public void Update(decimal unitPrice, decimal totalPayableAmount, string? note, bool hasPayments)
    {
        CheckRule(new PriceCannotBeChangedAfterPaymentRule(hasPayments));
        UnitPrice = unitPrice;
        TotalPayableAmount = totalPayableAmount;
        Note = note;
        LastModifiedDate = DateTime.UtcNow; 
    }
}
