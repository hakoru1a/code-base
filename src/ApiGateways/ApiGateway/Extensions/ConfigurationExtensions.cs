using ApiGateway.Configurations;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuration setup
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Configure all gateway options from configuration
    /// </summary>
    public static (ServicesOptions, OAuthOptions) ConfigureGatewayOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure services options
        services.Configure<ServicesOptions>(
            configuration.GetSection(ServicesOptions.SectionName));

        // Configure OAuth options
        services.Configure<OAuthOptions>(
            configuration.GetSection(OAuthOptions.SectionName));

        // Get options for immediate use
        var servicesOptions = Infrastructure.Extensions.ConfigurationExtensions
            .GetOptions<ServicesOptions>(services, ServicesOptions.SectionName);

        var oAuthOptions = Infrastructure.Extensions.ConfigurationExtensions
            .GetOptions<OAuthOptions>(services, OAuthOptions.SectionName);

        return (servicesOptions, oAuthOptions);
    }
}

