using Contracts.Domain;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.WeighingTickets.ValueObjects;

namespace TLBIOMASS.Domain.WeighingTickets;

public class WeighingTicket : EntityBase<int>
{
    public string TicketNumber { get; private set; } = string.Empty;
    public int CustomerId { get; private set; }
    public int MaterialId { get; private set; }
    public string VehiclePlate { get; private set; } = string.Empty;
    public string TicketType { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string MaterialName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public WeightInfo Weights { get; private set; } = null!;
    public long Price { get; private set; }
    public long TotalAmount { get; private set; }
    public DateTime? FirstWeighingTime { get; private set; }
    public DateTime? SecondWeighingTime { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public DateTime CreatedDate { get; private set; } // ngay_tao
    public string? FSCClassification { get; private set; }
    public bool HasOriginProfile { get; private set; }
    public TicketImages Images { get; private set; } = null!;
    public string? Note { get; private set; }
    public string? QualityStatus { get; private set; }
    public string? CreatedByString { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; } 
    public int? ReceiverId { get; private set; }

    // Navigation properties
    public virtual WeighingTicketPayment? FinalPayment { get; private set; }
    public virtual ICollection<PaymentDetail> PaymentDetails { get; private set; } = new List<PaymentDetail>();
    public virtual Receiver? Receiver { get; private set; }
    public virtual Material? Material { get; private set; }

    // Calculated properties (Not mapped to DB)
    public decimal RemainingAmount => PaymentDetails.Any() 
        ? PaymentDetails.OrderByDescending(pd => pd.CreatedDate).ThenByDescending(pd => pd.Id).First().PaymentAmount.RemainingAmount 
        : (FinalPayment != null ? FinalPayment.TotalPayableAmount : (decimal)TotalAmount);

    public bool IsPaid => PaymentDetails.Any();

    public bool IsFullyPaid => PaymentDetails.Any(pd => pd.PaymentAmount.RemainingAmount == 0);

    protected WeighingTicket() { }

    private WeighingTicket(
        string ticketNumber,
        int customerId,
        int materialId,
        string vehiclePlate,
        string ticketType,
        string customerName,
        string materialName,
        string? phoneNumber,
        WeightInfo weights,
        long price,
        long totalAmount,
        DateTime? firstWeighingTime,
        DateTime? secondWeighingTime,
        DateTime? paymentDate,
        DateTime createdDate,
        string? fscClassification,
        bool hasOriginProfile,
        TicketImages images,
        string? note,
        string? qualityStatus,
        string? createdByString,
        DateTime createdAt,
        int? receiverId)
    {
        TicketNumber = ticketNumber;
        CustomerId = customerId;
        MaterialId = materialId;
        VehiclePlate = vehiclePlate;
        TicketType = ticketType;
        CustomerName = customerName;
        MaterialName = materialName;
        PhoneNumber = phoneNumber;
        Weights = weights;
        Price = price;
        TotalAmount = totalAmount;
        FirstWeighingTime = firstWeighingTime;
        SecondWeighingTime = secondWeighingTime;
        PaymentDate = paymentDate;
        CreatedDate = createdDate;
        FSCClassification = fscClassification;
        HasOriginProfile = hasOriginProfile;
        Images = images;
        Note = note;
        QualityStatus = qualityStatus;
        CreatedByString = createdByString;
        CreatedAt = createdAt;
        ReceiverId = receiverId;
    }

    public static WeighingTicket Create(
        string ticketNumber,
        int customerId,
        int materialId,
        string vehiclePlate,
        string ticketType,
        string customerName,
        string materialName,
        string? phoneNumber,
        WeightInfo weights,
        long price,
        long totalAmount,
        DateTime? firstWeighingTime,
        DateTime? secondWeighingTime,
        DateTime? paymentDate,
        DateTime createdDate,
        string? fscClassification,
        bool hasOriginProfile,
        TicketImages images,
        string? note,
        string? qualityStatus,
        string? createdByString,
        int? receiverId)
    {
        return new WeighingTicket(
            ticketNumber,
            customerId,
            materialId,
            vehiclePlate,
            ticketType,
            customerName,
            materialName,
            phoneNumber,
            weights,
            price,
            totalAmount,
            firstWeighingTime,
            secondWeighingTime,
            paymentDate,
            createdDate,
            fscClassification,
            hasOriginProfile,
            images,
            note,
            qualityStatus,
            createdByString,
            DateTime.UtcNow,
            receiverId);
    }

    public void Update(
        int customerId,
        int materialId,
        string vehiclePlate,
        string customerName,
        string materialName,
        string? phoneNumber,
        WeightInfo weights,
        long price,
        long totalAmount,
        DateTime? firstWeighingTime,
        DateTime? secondWeighingTime,
        DateTime? paymentDate,
        string? fscClassification,
        bool hasOriginProfile,
        TicketImages images,
        string? note,
        string? qualityStatus,
        int? receiverId)
    {
        CustomerId = customerId;
        MaterialId = materialId;
        VehiclePlate = vehiclePlate;
        CustomerName = customerName;
        MaterialName = materialName;
        PhoneNumber = phoneNumber;
        Weights = weights;
        Price = price;
        TotalAmount = totalAmount;
        FirstWeighingTime = firstWeighingTime;
        SecondWeighingTime = secondWeighingTime;
        PaymentDate = paymentDate;
        FSCClassification = fscClassification;
        HasOriginProfile = hasOriginProfile;
        Images = images;
        Note = note;
        QualityStatus = qualityStatus;
        ReceiverId = receiverId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignReceiver(int receiverId)
    {
        ReceiverId = receiverId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignReceiver()
    {
        ReceiverId = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
