using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// Interface cho OAuth Client
/// Handle communication với Keycloak Token Endpoint
/// </summary>
public interface IOAuthClient
{
    /// <summary>
    /// Exchange authorization code lấy tokens
    /// Gọi sau khi user login thành công và Keycloak redirect về với code
    /// </summary>
    /// <param name="code">Authorization code từ Keycloak</param>
    /// <param name="codeVerifier">PKCE code verifier</param>
    /// <param name="redirectUri">Redirect URI (phải match với lúc request)</param>
    /// <returns>Token response chứa access_token, refresh_token, id_token</returns>
    Task<TokenResponse> ExchangeCodeForTokensAsync(
        string code,
        string codeVerifier,
        string redirectUri);

    /// <summary>
    /// Refresh access token bằng refresh token
    /// Gọi khi access token sắp hết hạn hoặc đã hết hạn
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>Token response với access_token mới</returns>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoke tokens (logout)
    /// Gọi khi user logout để invalidate tokens
    /// </summary>
    /// <param name="refreshToken">Refresh token cần revoke</param>
    Task RevokeTokenAsync(string refreshToken);

    /// <summary>
    /// Build Authorization URL để redirect browser tới Keycloak
    /// </summary>
    /// <param name="pkceData">PKCE data chứa state, code_challenge</param>
    /// <param name="redirectUri">Full redirect URI (scheme://host/path)</param>
    /// <returns>Authorization URL với query parameters</returns>
    string BuildAuthorizationUrl(Models.PkceData pkceData, string redirectUri);
}

