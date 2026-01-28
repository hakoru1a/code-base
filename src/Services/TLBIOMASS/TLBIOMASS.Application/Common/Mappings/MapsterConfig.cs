using Mapster;
using Shared.DTOs;
using Shared.DTOs.Agency;
using Shared.DTOs.Landowner;
using Shared.DTOs.Material;
using Shared.DTOs.MaterialRegion;
using Shared.DTOs.Company;
using Shared.DTOs.WeighingTicket;
using Shared.DTOs.Receiver;
using Shared.DTOs.Payment;
using Shared.DTOs.WeighingTicketCancel;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.Companies;
using TLBIOMASS.Domain.Payments;
using System.Linq;

namespace TLBIOMASS.Application.Common.Mappings;

public static class MapsterConfig
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        // Global conversion for DateTimeOffset (from EntityAuditBase) to DateTime (for DTOs if needed)
        // Note: BaseResponseDto uses DateTimeOffset, so this is mainly for custom properties
        TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig().MapWith(src => src.DateTime);
        TypeAdapterConfig<DateTimeOffset?, DateTime?>.NewConfig().MapWith(src => src.HasValue ? src.Value.DateTime : (DateTime?)null);

        ConfigureAgencyMappings();
        ConfigureLandownerMappings();
        ConfigureMaterialMappings();
        ConfigureMaterialRegionMappings();
        ConfigureReceiverMappings();
        ConfigureCompanyMappings();
        ConfigureWeighingTicketMappings();
        ConfigurePaymentDetailMappings();
        ConfigureWeighingTicketCancelMappings();
    }

    private static void ConfigurePaymentDetailMappings()
    {
        TypeAdapterConfig<PaymentDetail, PaymentDetailResponseDto>
            .NewConfig()
            .Map(dest => dest.TicketNumber, src => src.WeighingTicket != null ? src.WeighingTicket.TicketNumber : null);
    }

    private static void ConfigureWeighingTicketMappings()
    {
        TypeAdapterConfig<WeighingTicket, WeighingTicketResponseDto>
            .NewConfig()
            .Map(dest => dest.CreatedDate, src => src.CreatedDate)
            .Map(dest => dest.LastModifiedDate, src => src.UpdatedAt)
            .Map(dest => dest.FinalUnitPrice, src => src.FinalPayment != null ? src.FinalPayment.UnitPrice : (decimal?)null)
            .Map(dest => dest.FinalTotalAmount, src => src.FinalPayment != null ? src.FinalPayment.TotalPayableAmount : (decimal?)null)
            .Map(dest => dest.RemainingAmount, src => src.RemainingAmount)
            .Map(dest => dest.IsFullyPaid, src => src.IsFullyPaid)
            .Map(dest => dest.IsPaid, src => src.IsPaid);
    }

    private static void ConfigureCompanyMappings()
    {
        // Company -> CompanyResponseDto (Id, CreatedDate, LastModifiedDate map automatically)
        TypeAdapterConfig<Company, CompanyResponseDto>.NewConfig();

        // CompanyCreateDto -> Company (factory method)
        TypeAdapterConfig<CompanyCreateDto, Company>
            .NewConfig()
            .ConstructUsing(src => Company.Create(
                src.CompanyName, 
                src.Address, 
                src.TaxCode, 
                src.Representative, 
                src.Position,
                src.PhoneNumber, 
                src.Email, 
                src.IdentityCardNo, 
                src.IssuePlace, 
                src.IssueDate));
    }

    private static void ConfigureAgencyMappings()
    {
        TypeAdapterConfig<Agency, AgencyResponseDto>.NewConfig();
        TypeAdapterConfig<AgencyCreateDto, Agency>.NewConfig()
            .ConstructUsing(src => Agency.Create(
                src.Name, src.Phone, src.Email, src.Address, src.BankAccount,
                src.BankName, src.IdentityCard, src.IssuePlace, src.IssueDate, src.IsActive));
    }

    private static void ConfigureLandownerMappings()
    {
        TypeAdapterConfig<Landowner, LandownerResponseDto>.NewConfig();
        TypeAdapterConfig<LandownerCreateDto, Landowner>.NewConfig()
            .ConstructUsing(src => Landowner.Create(
                src.Name, src.Phone, src.Email, src.Address, src.BankAccount,
                src.BankName, src.IdentityCardNo, src.IssuePlace, src.IssueDate, src.DateOfBirth, src.IsActive));
    }

    private static void ConfigureMaterialMappings()
    {
        TypeAdapterConfig<Material, MaterialResponseDto>
            .NewConfig()
            .Map(dest => dest.CreatedDate, src => src.CreatedAt)
            .Map(dest => dest.LastModifiedDate, src => src.UpdatedAt);

        TypeAdapterConfig<MaterialCreateDto, Material>.NewConfig()
            .ConstructUsing(src => Material.Create(
                src.Name, src.Unit, src.Description, src.ProposedImpurityDeduction, src.IsActive));
    }

    private static void ConfigureMaterialRegionMappings()
    {
        TypeAdapterConfig<MaterialRegion, MaterialRegionResponseDto>.NewConfig();
        TypeAdapterConfig<MaterialRegionCreateDto, MaterialRegion>.NewConfig()
            .ConstructUsing(src => MaterialRegion.Create(
                src.RegionName, src.Address, src.Latitude, src.Longitude, src.AreaHa, src.CertificateID, src.OwnerId));
    }

    private static void ConfigureReceiverMappings()
    {
        TypeAdapterConfig<Receiver, ReceiverResponseDto>
            .NewConfig()
            .Map(dest => dest.CreatedDate, src => src.CreatedAt)
            .Map(dest => dest.LastModifiedDate, src => src.UpdatedAt);

        TypeAdapterConfig<ReceiverCreateDto, Receiver>.NewConfig()
            .ConstructUsing(src => Receiver.Create(
                src.Name, src.Phone, src.BankAccount, src.BankName, src.IdentityNumber,
                src.IssuedDate, src.IssuedPlace, src.Address, src.IsDefault, src.IsActive,
                src.Note, src.DateOfBirth));
    }

    private static void ConfigureWeighingTicketCancelMappings()
    {
        TypeAdapterConfig<TLBIOMASS.Domain.WeighingTicketCancels.WeighingTicketCancel, WeighingTicketCancelResponseDto>.NewConfig();
    }
}
