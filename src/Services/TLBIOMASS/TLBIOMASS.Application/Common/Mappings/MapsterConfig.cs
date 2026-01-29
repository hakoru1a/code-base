using Mapster;
using Shared.DTOs;
using Shared.DTOs.Agency;
using Shared.DTOs.Customer;
using Shared.DTOs.Landowner;
using Shared.DTOs.Material;
using Shared.DTOs.MaterialRegion;
using Shared.DTOs.Company;
using Shared.DTOs.WeighingTicket;
using Shared.DTOs.Receiver;
using Shared.DTOs.Payment;
using Shared.DTOs.BankAccount;
using Shared.DTOs.WeighingTicketCancel;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.MaterialRegions;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;
using TLBIOMASS.Domain.Materials.ValueObjects;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.Companies;
using TLBIOMASS.Domain.Companies.ValueObjects;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.BankAccounts;
using System.Linq;

using TLBIOMASS.Domain.WeighingTicketCancels;

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
        ConfigureCustomerMappings();
        ConfigureLandownerMappings();
        ConfigureMaterialMappings();
        ConfigureMaterialRegionMappings();
        ConfigureReceiverMappings();
        ConfigureCompanyMappings();
        ConfigureWeighingTicketMappings();
        ConfigurePaymentDetailMappings();
        ConfigureWeighingTicketCancelMappings();
        ConfigureBankAccountMappings();
    }

    private static void ConfigureBankAccountMappings()
    {
        TypeAdapterConfig<BankAccount, BankAccountResponseDto>.NewConfig();
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
            .Map(dest => dest.IsPaid, src => src.IsPaid)
            .Map(dest => dest.WeightIn, src => src.Weights.WeightIn)
            .Map(dest => dest.WeightOut, src => src.Weights.WeightOut)
            .Map(dest => dest.NetWeight, src => src.Weights.NetWeight)
            .Map(dest => dest.ImpurityDeduction, src => src.Weights.ImpurityDeduction)
            .Map(dest => dest.PayableWeight, src => src.Weights.PayableWeight)
            .Map(dest => dest.VehicleFrontImage, src => src.Images.VehicleFrontImage)
            .Map(dest => dest.VehicleBodyImage, src => src.Images.VehicleBodyImage)
            .Map(dest => dest.VehicleRearImage, src => src.Images.VehicleRearImage);
    }

    private static void ConfigureCompanyMappings()
    {
        // Company -> CompanyResponseDto
        TypeAdapterConfig<Company, CompanyResponseDto>.NewConfig()
            .Map(dest => dest.Representative, src => src.Representative != null ? src.Representative.Name : null)
            .Map(dest => dest.Position, src => src.Representative != null ? src.Representative.Position : null)
            .Map(dest => dest.Address, src => src.Contact != null ? src.Contact.Address : null)
            .Map(dest => dest.PhoneNumber, src => src.Contact != null ? src.Contact.Phone : null)
            .Map(dest => dest.Email, src => src.Contact != null ? src.Contact.Email : null)
            .Map(dest => dest.IdentityCardNo, src => src.Identity != null ? src.Identity.IdentityNumber : null)
            .Map(dest => dest.IssuePlace, src => src.Identity != null ? src.Identity.IssuePlace : null)
            .Map(dest => dest.IssueDate, src => src.Identity != null ? src.Identity.IssueDate : null);

        // CompanyCreateDto -> Company (factory method)
        TypeAdapterConfig<CompanyCreateDto, Company>
            .NewConfig()
            .ConstructUsing(src => Company.Create(
                src.CompanyName, 
                src.TaxCode, 
                new RepresentativeInfo(src.Representative, src.Position),
                new ContactInfo(src.PhoneNumber, src.Email, src.Address, null), 
                new IdentityInfo(src.IdentityCardNo, src.IssuePlace, src.IssueDate, null)));
    }

    private static void ConfigureCustomerMappings()
    {
        TypeAdapterConfig<Customer, CustomerResponseDto>.NewConfig()
            .Map(d => d.Phone, s => s.Contact != null ? s.Contact.Phone : null)
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.Email, s => s.Contact != null ? s.Contact.Email : null)
            .Map(d => d.Note, s => s.Contact != null ? s.Contact.Note : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedDate.UtcDateTime)
            .Map(dest => dest.UpdatedAt, src => src.LastModifiedDate.HasValue ? src.LastModifiedDate.Value.UtcDateTime : (DateTime?)null);

        TypeAdapterConfig<CustomerCreateDto, Customer>.NewConfig()
            .ConstructUsing(src => Customer.Create(
                src.Name,
                new ContactInfo(src.Phone, src.Email, src.Address, src.Note),
                src.TaxCode));
    }

    private static void ConfigureAgencyMappings()
    {
        TypeAdapterConfig<Agency, AgencyResponseDto>.NewConfig()
            .Map(d => d.Phone, s => s.Contact != null ? s.Contact.Phone : null)
            .Map(d => d.Email, s => s.Contact != null ? s.Contact.Email : null)
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.BankAccount, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.AccountNumber).FirstOrDefault())
            .Map(d => d.BankName, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.BankName).FirstOrDefault())
            .Map(d => d.IdentityCard, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuePlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.IssueDate, s => s.Identity != null ? s.Identity.IssueDate : null);

        TypeAdapterConfig<AgencyCreateDto, Agency>.NewConfig()
            .ConstructUsing(src => Agency.Create(
                src.Name,
                new ContactInfo(src.Phone, src.Email, src.Address, null),
                new IdentityInfo(src.IdentityCard, src.IssuePlace, src.IssueDate, null),
                src.IsActive));
    }

    private static void ConfigureLandownerMappings()
    {
        TypeAdapterConfig<Landowner, LandownerResponseDto>.NewConfig()
            .Map(d => d.Phone, s => s.Contact != null ? s.Contact.Phone : null)
            .Map(d => d.Email, s => s.Contact != null ? s.Contact.Email : null)
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.BankAccount, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.AccountNumber).FirstOrDefault())
            .Map(d => d.BankName, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.BankName).FirstOrDefault())
            .Map(d => d.IdentityCardNo, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuePlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.IssueDate, s => s.Identity != null ? s.Identity.IssueDate : null)
            .Map(d => d.DateOfBirth, s => s.Identity != null ? s.Identity.DateOfBirth : null);

        TypeAdapterConfig<LandownerCreateDto, Landowner>.NewConfig()
            .ConstructUsing(src => Landowner.Create(
                src.Name,
                new ContactInfo(src.Phone, src.Email, src.Address, null),
                new IdentityInfo(src.IdentityCardNo, src.IssuePlace, src.IssueDate, src.DateOfBirth),
                src.IsActive));
    }

    private static void ConfigureMaterialMappings()
    {
        TypeAdapterConfig<Material, MaterialResponseDto>.NewConfig();
        TypeAdapterConfig<MaterialCreateDto, Material>.NewConfig()
            .ConstructUsing(src => Material.Create(
                new MaterialSpec(src.Name, src.Unit, src.Description, src.ProposedImpurityDeduction),
                src.IsActive));
    }

    private static void ConfigureMaterialRegionMappings()
    {
        TypeAdapterConfig<MaterialRegion, MaterialRegionResponseDto>.NewConfig()
            .Map(d => d.RegionName, s => s.Detail.RegionName)
            .Map(d => d.Address, s => s.Detail.Address)
            .Map(d => d.Latitude, s => s.Detail.Latitude)
            .Map(d => d.Longitude, s => s.Detail.Longitude)
            .Map(d => d.AreaHa, s => s.Detail.AreaHa)
            .Map(d => d.CertificateID, s => s.Detail.CertificateId);

        TypeAdapterConfig<MaterialRegionCreateDto, MaterialRegion>.NewConfig()
            .ConstructUsing(src => MaterialRegion.Create(
                new RegionDetail(src.RegionName, src.Address, src.Latitude, src.Longitude, src.AreaHa, src.CertificateID),
                src.OwnerId));
    }

    private static void ConfigureReceiverMappings()
    {
        TypeAdapterConfig<Receiver, ReceiverResponseDto>.NewConfig()
            .Map(d => d.Phone, s => s.Contact != null ? s.Contact.Phone : null)
            .Map(d => d.Email, s => s.Contact != null ? s.Contact.Email : null)
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.Note, s => s.Contact != null ? s.Contact.Note : null)
            .Map(d => d.IdentityNumber, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuedDate, s => s.Identity != null ? s.Identity.IssueDate : null)
            .Map(d => d.IssuedPlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.DateOfBirth, s => s.Identity != null ? s.Identity.DateOfBirth : null)
            .Map(d => d.BankAccount, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.AccountNumber).FirstOrDefault())
            .Map(d => d.BankName, s => s.BankAccounts.Where(x => x.IsDefault).Select(x => x.BankName).FirstOrDefault())
            .Map(d => d.CreatedDate, s => s.CreatedAt)
            .Map(d => d.LastModifiedDate, s => s.UpdatedAt);

        TypeAdapterConfig<ReceiverCreateDto, Receiver>.NewConfig()
            .ConstructUsing(src => Receiver.Create(
                src.Name,
                new ContactInfo(src.Phone, null, src.Address, src.Note),
                new IdentityInfo(src.IdentityNumber, src.IssuedPlace, src.IssuedDate, src.DateOfBirth),
                src.IsDefault,
                src.IsActive));
    }

    private static void ConfigureWeighingTicketCancelMappings()
    {
        TypeAdapterConfig<WeighingTicketCancel, WeighingTicketCancelResponseDto>.NewConfig();
    }
}
