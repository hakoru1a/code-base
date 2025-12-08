using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// OAuth Client interface - communicate với Keycloak
/// </summary>
public interface IOAuthClient
{
    /// <summary>
    /// Build authorization URL để redirect browser tới Keycloak
    /// </summary>
    string BuildAuthorizationUrl(PkceData pkceData, string redirectUri);

    /// <summary>
    /// Exchange authorization code để lấy tokens
    /// </summary>
    Task<TokenResponse> ExchangeCodeForTokensAsync(string code, string codeVerifier, string redirectUri);

    /// <summary>
    /// Refresh access token
    /// </summary>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoke token (logout)
    /// </summary>
    Task RevokeTokenAsync(string refreshToken);
}
