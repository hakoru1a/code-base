namespace ApiGateway.Configurations;

/// <summary>
/// OAuth configuration options
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "OAuth";

    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "/auth/signin-oidc";
    public string PostLogoutRedirectUri { get; set; } = "/";
    public string[] Scopes { get; set; } = new[] { "openid", "profile", "email", "profile_extended" }; // profile_extended is a custom scope that is not standard OIDC scope
    public string ResponseType { get; set; } = "code";
    public bool UsePkce { get; set; } = true;
    public string WebAppUrl { get; set; } = "http://localhost:3000";
    public int SessionSlidingExpirationMinutes { get; set; } = 60;
    public int SessionAbsoluteExpirationMinutes { get; set; } = 480;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string EndSessionEndpoint { get; set; } = string.Empty;
    public int RefreshTokenBeforeExpirationSeconds { get; set; } = 60;
    public string InstanceName { get; set; } = "ApiGateway_";
    public int PkceExpirationMinutes { get; set; } = 10;
    public string[] CorsAllowedOrigins { get; set; } = Array.Empty<string>();
}












