using Shared.SeedWork;

namespace Shared.DTOs.WeighingTicket;

public class WeighingTicketFilterDto : RequestParameter
{
    // SearchTerms via base class. Can search by TicketNumber, VehiclePlate, CustomerName, MaterialName
    public int? CustomerId { get; set; }
    public int? MaterialId { get; set; }
    public string? TicketType { get; set; }
    public bool? IsPaid { get; set; }
    public bool? IsFullyPaid { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
