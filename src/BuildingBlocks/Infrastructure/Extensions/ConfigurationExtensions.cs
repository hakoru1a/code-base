using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Validate configuration options using IValidateOptions pattern
    /// Throws exception if validation fails, ensuring fail-fast behavior
    /// 
    /// CÁCH DÙNG:
    /// builder.Services.ValidateConfiguration<DatabaseSettings>("DatabaseSettings");
    /// 
    /// LƯU Ý: Configuration sẽ được validate khi service provider được build
    /// </summary>
    public static IServiceCollection ValidateConfiguration<T>(
        this IServiceCollection services,
        string sectionName) where T : class
    {
        services.AddOptions<T>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Thay thế các biến môi trường trong configuration values
    /// Hỗ trợ cú pháp ${VARIABLE_NAME} trong appsettings.json
    /// 
    /// CÁCH DÙNG:
    /// builder.Configuration.SubstituteEnvironmentVariables();
    /// 
    /// VÍ DỤ:
    /// appsettings.json: "ConnectionString": "${DATABASE_CONNECTIONSTRING}"
    /// .env: DATABASE_CONNECTIONSTRING=Server=localhost;...
    /// Kết quả: "ConnectionString": "Server=localhost;..."
    /// </summary>
    public static IConfigurationBuilder SubstituteEnvironmentVariables(this IConfigurationBuilder builder)
    {
        // Add a custom configuration source that wraps existing sources
        builder.Add(new EnvironmentVariableSubstitutionConfigurationSource());
        return builder;
    }

    /// <summary>
    /// Thay thế ${VARIABLE} trong một string value bằng giá trị từ environment variables
    /// </summary>
    private static string SubstituteEnvironmentVariables(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Pattern to match ${VARIABLE_NAME}
        var pattern = @"\$\{([^}]+)\}";
        return Regex.Replace(value, pattern, match =>
        {
            var envVarName = match.Groups[1].Value;
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            return envValue ?? match.Value; // Return original if env var not found
        });
    }
}

/// <summary>
/// Custom configuration source để thay thế ${VARIABLE} trong configuration values
/// </summary>
internal class EnvironmentVariableSubstitutionConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        // Build configuration from all previous sources
        var tempConfig = new ConfigurationBuilder();
        foreach (var source in builder.Sources.Where(s => !(s is EnvironmentVariableSubstitutionConfigurationSource)))
        {
            tempConfig.Sources.Add(source);
        }
        var baseConfig = tempConfig.Build();

        return new EnvironmentVariableSubstitutionConfigurationProvider(baseConfig);
    }
}

/// <summary>
/// Configuration provider để thay thế ${VARIABLE} trong configuration values
/// </summary>
internal class EnvironmentVariableSubstitutionConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigurationRoot _baseConfiguration;

    public EnvironmentVariableSubstitutionConfigurationProvider(IConfigurationRoot baseConfiguration)
    {
        _baseConfiguration = baseConfiguration;
    }

    public override void Load()
    {
        // Load all values from base configuration and substitute environment variables
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        LoadRecursive(_baseConfiguration, "", data);
        Data = data;
    }

    private void LoadRecursive(IConfiguration config, string prefix, Dictionary<string, string?> data)
    {
        foreach (var child in config.GetChildren())
        {
            var key = string.IsNullOrEmpty(prefix) ? child.Key : $"{prefix}:{child.Key}";

            if (child.GetChildren().Any())
            {
                // Recursively load nested sections
                LoadRecursive(child, key, data);
            }
            else
            {
                // Get value and substitute environment variables
                var value = child.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    value = SubstituteEnvironmentVariables(value);
                }
                data[key] = value;
            }
        }
    }

    private static string SubstituteEnvironmentVariables(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Pattern to match ${VARIABLE_NAME}
        var pattern = @"\$\{([^}]+)\}";
        return Regex.Replace(value, pattern, match =>
        {
            var envVarName = match.Groups[1].Value;
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            return envValue ?? match.Value; // Return original if env var not found
        });
    }
}
