namespace TLBIOMASS.Domain.Payments.ValueObjects;

public record PaymentInfo(
    string PaymentCode,
    DateTime PaymentDate,
    DateTime? CustomerPaymentDate,
    string? Note);
