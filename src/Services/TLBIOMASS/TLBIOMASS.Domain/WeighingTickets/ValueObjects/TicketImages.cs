namespace TLBIOMASS.Domain.WeighingTickets.ValueObjects;

public record TicketImages(
    string? VehicleFrontImage = null,
    string? VehicleBodyImage = null,
    string? VehicleRearImage = null);
