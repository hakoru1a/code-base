using Mapster;

namespace TLBIOMASS.Application.Common.Mappings;

/// <summary>
/// Optimized Mapster configuration for TLBIOMASS service
/// Follows DDD principles with clean mapping logic
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    /// Configure all mappings for the TLBIOMASS service
    /// </summary>
    public static void ConfigureMappings()
    {
        // Configure global settings for better performance
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        // TODO: Add specific mappings as needed
        // ConfigureEntityMappings();
    }

}
