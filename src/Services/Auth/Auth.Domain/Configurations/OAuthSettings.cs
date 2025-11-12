namespace Auth.Domain.Configurations;

/// <summary>
/// Cấu hình OAuth 2.0 / OpenID Connect
/// </summary>
public class OAuthSettings
{
    public const string SectionName = "OAuth";
    
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "/auth/signin-oidc";
    public string PostLogoutRedirectUri { get; set; } = "/";
    public string[] Scopes { get; set; } = new[] { "openid", "profile", "email" };
    public string ResponseType { get; set; } = "code";
    public bool UsePkce { get; set; } = true;
    public string WebAppUrl { get; set; } = string.Empty;
    public int SessionSlidingExpirationMinutes { get; set; } = 60;
    public int SessionAbsoluteExpirationMinutes { get; set; } = 480;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string EndSessionEndpoint { get; set; } = string.Empty;
    public int RefreshTokenBeforeExpirationSeconds { get; set; } = 60;
}
