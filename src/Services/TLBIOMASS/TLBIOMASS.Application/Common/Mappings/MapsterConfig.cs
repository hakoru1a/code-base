using Mapster;
using Shared.DTOs.Agency;
using Shared.DTOs.Customer;
using Shared.DTOs.Landowner;
using Shared.DTOs.Material;
using Shared.DTOs.MaterialRegion;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.MaterialRegions;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;
using TLBIOMASS.Domain.Materials.ValueObjects;
using TLBIOMASS.Domain.Receivers;

namespace TLBIOMASS.Application.Common.Mappings;

public static class MapsterConfig
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        ConfigureAgencyMappings();
        ConfigureCustomerMappings();
        ConfigureLandownerMappings();
        ConfigureMaterialMappings();
        ConfigureMaterialRegionMappings();
        ConfigureReceiverMappings();
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
            .Map(d => d.BankAccount, s => s.Bank != null ? s.Bank.BankAccount : null)
            .Map(d => d.BankName, s => s.Bank != null ? s.Bank.BankName : null)
            .Map(d => d.IdentityCard, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuePlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.IssueDate, s => s.Identity != null ? s.Identity.IssueDate : null);

        TypeAdapterConfig<AgencyCreateDto, Agency>.NewConfig()
            .ConstructUsing(src => Agency.Create(
                src.Name,
                new ContactInfo(src.Phone, src.Email, src.Address),
                new BankInfo(src.BankAccount, src.BankName),
                new IdentityInfo(src.IdentityCard, src.IssuePlace, src.IssueDate),
                src.IsActive));
    }

    private static void ConfigureLandownerMappings()
    {
        TypeAdapterConfig<Landowner, LandownerResponseDto>.NewConfig()
            .Map(d => d.Phone, s => s.Contact != null ? s.Contact.Phone : null)
            .Map(d => d.Email, s => s.Contact != null ? s.Contact.Email : null)
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.BankAccount, s => s.Bank != null ? s.Bank.BankAccount : null)
            .Map(d => d.BankName, s => s.Bank != null ? s.Bank.BankName : null)
            .Map(d => d.IdentityCardNo, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuePlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.IssueDate, s => s.Identity != null ? s.Identity.IssueDate : null)
            .Map(d => d.DateOfBirth, s => s.Identity != null ? s.Identity.DateOfBirth : null);

        TypeAdapterConfig<LandownerCreateDto, Landowner>.NewConfig()
            .ConstructUsing(src => Landowner.Create(
                src.Name,
                new ContactInfo(src.Phone, src.Email, src.Address),
                new BankInfo(src.BankAccount, src.BankName),
                new IdentityInfo(src.IdentityCardNo, src.IssuePlace, src.IssueDate, src.DateOfBirth),
                src.IsActive));
    }

    private static void ConfigureMaterialMappings()
    {
        TypeAdapterConfig<Material, MaterialResponseDto>.NewConfig()
            .Map(d => d.Name, s => s.Spec.Name)
            .Map(d => d.Unit, s => s.Spec.Unit)
            .Map(d => d.Description, s => s.Spec.Description)
            .Map(d => d.ProposedImpurityDeduction, s => s.Spec.ProposedImpurityDeduction);

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
            .Map(d => d.Address, s => s.Contact != null ? s.Contact.Address : null)
            .Map(d => d.Note, s => s.Contact != null ? s.Contact.Note : null)
            .Map(d => d.BankAccount, s => s.Bank != null ? s.Bank.BankAccount : null)
            .Map(d => d.BankName, s => s.Bank != null ? s.Bank.BankName : null)
            .Map(d => d.IdentityNumber, s => s.Identity != null ? s.Identity.IdentityNumber : null)
            .Map(d => d.IssuedDate, s => s.Identity != null ? s.Identity.IssueDate : null)
            .Map(d => d.IssuedPlace, s => s.Identity != null ? s.Identity.IssuePlace : null)
            .Map(d => d.DateOfBirth, s => s.Identity != null ? s.Identity.DateOfBirth : null);

        TypeAdapterConfig<ReceiverCreateDto, Receiver>.NewConfig()
            .ConstructUsing(src => Receiver.Create(
                src.Name,
                new ContactInfo(src.Phone, null, src.Address, src.Note),
                new BankInfo(src.BankAccount, src.BankName),
                new IdentityInfo(src.IdentityNumber, src.IssuedPlace, src.IssuedDate, src.DateOfBirth),
                src.IsDefault,
                src.IsActive));
    }
}
