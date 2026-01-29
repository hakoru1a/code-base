namespace TLBIOMASS.Domain.Payments.ValueObjects;

public record PaymentAmount(
    decimal? Amount,
    decimal? RemainingAmount);
