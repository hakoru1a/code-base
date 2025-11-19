using Mapster;
using Shared.DTOs.Product;
using Base.Domain.Entities;

namespace Base.Application.Common;

/// <summary>
/// Mapster configuration for Base service mappings
/// High-performance alternative to AutoMapper
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    /// Configure all mappings for the Base service
    /// </summary>
    public static void ConfigureMappings()
    {
        // Configure global settings for better performance
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);
        
        ConfigureProductMappings();
    }

    /// <summary>
    /// Configure Product entity mappings with smart property matching
    /// </summary>
    private static void ConfigureProductMappings()
    {
        // Product -> ProductDto (bidirectional mapping with ignore non-existing)
        TypeAdapterConfig<Product, ProductDto>
            .NewConfig()
            .IgnoreNonMapped(true); // Equivalent to IgnoreAllNonExisting

        TypeAdapterConfig<ProductDto, Product>
            .NewConfig()
            .IgnoreNonMapped(true); // Equivalent to IgnoreAllNonExisting
    }
}