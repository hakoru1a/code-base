using Shared.DTOs.Receiver;
using Shared.DTOs.Material;
using Contracts.Domain.Enums;

namespace Shared.DTOs.WeighingTicket;

public class WeighingTicketResponseDto : BaseResponseDto<int>
{
    public string TicketNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? ReceiverId { get; set; }
    public ReceiverResponseDto? Receiver { get; set; }
    public int MaterialId { get; set; }
    public MaterialResponseDto? Material { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int? WeightIn { get; set; }
    public int? WeightOut { get; set; }
    public int? NetWeight { get; set; }
    public int? ImpurityDeduction { get; set; }
    public int? PayableWeight { get; set; }
    public long Price { get; set; }
    public long TotalAmount { get; set; }
    public DateTime? FirstWeighingTime { get; set; }
    public DateTime? SecondWeighingTime { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? FSCClassification { get; set; }
    public bool HasOriginProfile { get; set; }
    public string? VehicleFrontImage { get; set; }
    public string? VehicleBodyImage { get; set; }
    public string? VehicleRearImage { get; set; }
    public string? Note { get; set; }
    public string? QualityStatus { get; set; }
    public string? CreatedByString { get; set; }
    public EntityStatus Status { get; set; }

    // Payment Info
    public decimal? FinalUnitPrice { get; set; }
    public decimal? FinalTotalAmount { get; set; }
    public decimal? RemainingAmount { get; set; }
    public bool IsPaid { get; set; }
    public bool IsFullyPaid { get; set; }
}
