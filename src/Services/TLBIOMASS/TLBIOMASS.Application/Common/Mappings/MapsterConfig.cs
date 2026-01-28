using Mapster;
using Shared.DTOs.Agency;
using Shared.DTOs.Landowner;
using Shared.DTOs.Material;
using Shared.DTOs.MaterialRegion;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Receivers;

namespace TLBIOMASS.Application.Common.Mappings;

public static class MapsterConfig
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        ConfigureAgencyMappings();
        ConfigureLandownerMappings();
        ConfigureMaterialMappings();
        ConfigureMaterialRegionMappings();
        ConfigureReceiverMappings();
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
        TypeAdapterConfig<Material, MaterialResponseDto>.NewConfig();
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
        TypeAdapterConfig<Receiver, ReceiverResponseDto>.NewConfig();
        TypeAdapterConfig<ReceiverCreateDto, Receiver>.NewConfig()
            .ConstructUsing(src => Receiver.Create(
                src.Name, src.Phone, src.BankAccount, src.BankName, src.IdentityNumber,
                src.IssuedDate, src.IssuedPlace, src.Address, src.IsDefault, src.IsActive,
                src.Note, src.DateOfBirth));
    }
}
