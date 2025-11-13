using Auth.Domain.Configurations;

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
        var authSettings = configuration
            .GetSection(AuthSettings.SectionName)
            .Get<AuthSettings>() ?? new AuthSettings();

        var oauthSettings = configuration
            .GetSection(OAuthSettings.SectionName)
            .Get<OAuthSettings>() ?? throw new InvalidOperationException("OAuth settings not configured");

        services.AddSingleton(authSettings);
        services.AddSingleton(oauthSettings);

        return (authSettings, oauthSettings);
    }
}





