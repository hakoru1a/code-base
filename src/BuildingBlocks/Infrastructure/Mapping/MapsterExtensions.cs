using Mapster;

namespace Infrastructure.Mapping;

/// <summary>
/// Mapster extension methods for enhanced mapping functionality
/// High-performance alternative to AutoMapper extensions
/// </summary>
public static class MapsterExtensions
{
    /// <summary>
    /// Bulk map a collection with better performance
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source collection</param>
    /// <returns>Mapped destination collection</returns>
    public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
        => source.Adapt<List<TDestination>>();

    /// <summary>
    /// Map single object with null safety
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <returns>Mapped destination object or null</returns>
    public static TDestination? MapTo<TDestination>(this object? source) where TDestination : class
        => source?.Adapt<TDestination>();

    /// <summary>
    /// Map object to existing instance (merge)
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <param name="destination">Existing destination instance</param>
    /// <returns>Updated destination instance</returns>
    public static TDestination MapToExisting<TDestination>(this object source, TDestination destination)
        => source.Adapt(destination);

    /// <summary>
    /// Configure global ignore for null values
    /// </summary>
    public static void ConfigureIgnoreNullValues()
    {
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }

    /// <summary>
    /// Configure global preserve reference setting
    /// </summary>
    public static void ConfigurePreserveReference()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
    }

    /// <summary>
    /// Configure global name matching strategy
    /// </summary>
    /// <param name="strategy">Name matching strategy</param>
    public static void ConfigureNameMatchingStrategy(NameMatchingStrategy strategy)
    {
        TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(strategy);
    }
}