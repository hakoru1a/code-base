namespace TLBIOMASS.Domain.WeighingTickets.ValueObjects;

public record WeightInfo(
    int WeightIn,
    int WeightOut,
    int NetWeight,
    int ImpurityDeduction,
    int PayableWeight);
