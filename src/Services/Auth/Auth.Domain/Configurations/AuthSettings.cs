using Shared.Configurations.Database;

namespace Auth.Domain.Configurations;

/// <summary>
/// Auth service specific settings
/// </summary>
public class AuthSettings : CacheSettings
{
    public const string SectionName = "Auth";

    public string InstanceName { get; set; } = "Auth_";
    public int SessionSlidingExpirationMinutes { get; set; } = 60;
    public int SessionAbsoluteExpirationMinutes { get; set; } = 480;
    public int PkceExpirationMinutes { get; set; } = 10;
    public int RefreshTokenBeforeExpirationSeconds { get; set; } = 60;
}
