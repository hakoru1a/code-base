using Auth.Domain.Configurations;
using Infrastructure.Extensions;

namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for configuration setup
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Configure all authentication settings from configuration
    /// </summary>
    public static (AuthSettings, OAuthSettings) ConfigureAuthSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authSettings = configuration.GetOptions<AuthSettings>(AuthSettings.SectionName);
        var oauthSettings = configuration.GetOptions<OAuthSettings>(OAuthSettings.SectionName);

        if (oauthSettings == null)
            throw new InvalidOperationException("OAuth settings not configured");

        services.AddSingleton(authSettings);
        services.AddSingleton(oauthSettings);

        return (authSettings, oauthSettings);
    }
}








