namespace TLBIOMASS.Domain.Payments.ValueObjects;

public record PaymentProcessStatus(
    bool IsPaid,
    bool IsLocked);
