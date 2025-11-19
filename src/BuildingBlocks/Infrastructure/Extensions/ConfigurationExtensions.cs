using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

/// <summary>
/// Extension methods cho IConfiguration - Bind configuration sections thành strongly-typed objects
/// 
/// MỤC ĐÍCH:
/// - Convert configuration sections (appsettings.json) thành C# objects
/// - Type-safe configuration access thay vì magic strings
/// - Reusable helper để đọc config ở nhiều nơi
/// 
/// SỬ DỤNG:
/// 1. Từ IServiceCollection:
///    var dbSettings = services.GetOptions<DatabaseSettings>("DatabaseSettings");
/// 
/// 2. Từ IConfiguration:
///    var dbSettings = configuration.GetOptions<DatabaseSettings>("DatabaseSettings");
/// 
/// VÍ DỤ appsettings.json:
/// {
///   "DatabaseSettings": {
///     "ConnectionStrings": "Server=localhost;...",
///     "DBProvider": "MySQL"
///   }
/// }
/// 
/// var settings = configuration.GetOptions<DatabaseSettings>("DatabaseSettings");
/// // settings.ConnectionStrings = "Server=localhost;..."
/// 
/// IMPACT:
/// + Type Safety: Compile-time checking, tránh typo trong section names
/// + IntelliSense: Auto-complete khi access configuration properties
/// + Easy Testing: Dễ dàng mock configuration objects
/// - Performance: Mỗi lần gọi tạo ServiceProvider mới (overload từ IServiceCollection)
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Bind configuration section thành strongly-typed object (từ IServiceCollection)
    /// 
    /// CÁCH DÙNG:
    /// var dbSettings = services.GetOptions<DatabaseSettings>("DatabaseSettings");
    /// 
    /// LƯU Ý: Tạo ServiceProvider mới mỗi lần gọi, chỉ dùng trong startup configuration
    /// </summary>
    public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
    {
        using var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);

        return options;
    }

    /// <summary>
    /// Bind configuration section thành strongly-typed object (từ IConfiguration)
    /// 
    /// CÁCH DÙNG:
    /// var cacheSettings = configuration.GetOptions<CacheSettings>("CacheSettings");
    /// 
    /// PHÙ HỢP: Dùng ở mọi nơi có IConfiguration, performance tốt hơn overload từ IServiceCollection
    /// </summary>
    public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        var section = configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);

        return options;
    }
}
