using Mapster;
using TLBIOMASS.Application.Features.Receivers.DTOs;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;
using TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;

namespace TLBIOMASS.Application.Common.Mappings;

public static class MapsterConfig
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        ConfigureReceiverMappings();
    }

    private static void ConfigureReceiverMappings()
    {
        // ReceiverCreateDto -> ReceiverCreateCommand
        TypeAdapterConfig<ReceiverCreateDto, CreateReceiverCommand>
            .NewConfig();

        // ReceiverUpdateDto -> ReceiverUpdateCommand
        TypeAdapterConfig<ReceiverUpdateDto, UpdateReceiverCommand>
            .NewConfig();

        // Receiver -> ReceiverResponseDto
        TypeAdapterConfig<Receiver, ReceiverResponseDto>
            .NewConfig();
    }
}