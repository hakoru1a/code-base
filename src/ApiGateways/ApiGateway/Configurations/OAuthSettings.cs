namespace ApiGateway.Configurations;

/// <summary>
/// Cấu hình OAuth 2.0 / OpenID Connect cho BFF pattern
/// Sử dụng Authorization Code Flow + PKCE để bảo mật
/// </summary>
public class OAuthSettings
{
    public const string SectionName = "OAuth";
    
    /// <summary>
    /// Authority URL của Identity Provider (Keycloak)
    /// VD: http://localhost:8080/realms/base-realm
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    
    /// <summary>
    /// Client ID đăng ký với Keycloak
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Client Secret (confidential client)
    /// BFF cần secret vì đây là backend server, không lộ ra browser
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Redirect URI sau khi user login thành công
    /// Phải match với configured redirect URI trong Keycloak
    /// VD: https://gateway.com/signin-oidc
    /// </summary>
    public string RedirectUri { get; set; } = "/signin-oidc";
    
    /// <summary>
    /// URI để redirect sau khi logout
    /// VD: https://gateway.com/signout-callback-oidc
    /// </summary>
    public string PostLogoutRedirectUri { get; set; } = "/";
    
    /// <summary>
    /// OpenID Connect scopes cần request
    /// Default: openid, profile, email
    /// </summary>
    public string[] Scopes { get; set; } = new[] { "openid", "profile", "email" };
    
    /// <summary>
    /// Response type cho OAuth flow
    /// Default: "code" (Authorization Code Flow)
    /// </summary>
    public string ResponseType { get; set; } = "code";
    
    /// <summary>
    /// Có sử dụng PKCE (Proof Key for Code Exchange) hay không
    /// HIGHLY RECOMMENDED để chống code interception attack
    /// </summary>
    public bool UsePkce { get; set; } = true;
    
    /// <summary>
    /// URL của webapp để redirect sau khi login thành công
    /// VD: https://webapp.com/dashboard
    /// </summary>
    public string WebAppUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian sliding expiration cho session (phút)
    /// Nếu user active, session sẽ được extend thêm
    /// </summary>
    public int SessionSlidingExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Thời gian absolute expiration cho session (phút)
    /// Dù user có active cũng phải re-login sau thời gian này
    /// </summary>
    public int SessionAbsoluteExpirationMinutes { get; set; } = 480; // 8 hours
    
    /// <summary>
    /// Token endpoint của Keycloak
    /// VD: http://localhost:8080/realms/base-realm/protocol/openid-connect/token
    /// </summary>
    public string TokenEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Authorization endpoint của Keycloak
    /// VD: http://localhost:8080/realms/base-realm/protocol/openid-connect/auth
    /// </summary>
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// End session endpoint (logout)
    /// VD: http://localhost:8080/realms/base-realm/protocol/openid-connect/logout
    /// </summary>
    public string EndSessionEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Số giây trước khi token expire để trigger refresh
    /// VD: 60 = refresh token trước 60s khi hết hạn
    /// </summary>
    public int RefreshTokenBeforeExpirationSeconds { get; set; } = 60;
}

