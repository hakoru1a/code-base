using ApiGateway.Configurations;
using ApiGateway.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của OAuth Client
/// Communicate với Keycloak OAuth endpoints
/// </summary>
public class OAuthClient : IOAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly OAuthSettings _oauthSettings;
    private readonly ILogger<OAuthClient> _logger;

    public OAuthClient(
        HttpClient httpClient,
        OAuthSettings oauthSettings,
        ILogger<OAuthClient> logger)
    {
        _httpClient = httpClient;
        _oauthSettings = oauthSettings;
        _logger = logger;
    }

    /// <summary>
    /// Exchange authorization code để lấy tokens
    /// Đây là bước cuối của Authorization Code Flow
    /// </summary>
    public async Task<TokenResponse> ExchangeCodeForTokensAsync(
        string code,
        string codeVerifier,
        string redirectUri)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for tokens");

            // 1. Prepare request body theo OAuth 2.0 spec
            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = _oauthSettings.ClientId,
                ["client_secret"] = _oauthSettings.ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier // PKCE
            };

            // 2. Call Keycloak Token Endpoint
            var response = await _httpClient.PostAsync(
                _oauthSettings.TokenEndpoint,
                new FormUrlEncodedContent(requestBody));

            var content = await response.Content.ReadAsStringAsync();

            // 3. Parse response
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to exchange code for tokens. Status: {Status}, Response: {Response}",
                    response.StatusCode,
                    content);

                throw new Exception($"Token exchange failed: {response.StatusCode}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Invalid token response: {Response}", content);
                throw new Exception("Invalid token response from Keycloak");
            }

            _logger.LogInformation("Successfully exchanged code for tokens");

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            throw;
        }
    }

    /// <summary>
    /// Refresh access token
    /// Call này được trigger tự động khi access token sắp expire
    /// </summary>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Refreshing access token");

            // 1. Prepare request body
            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _oauthSettings.ClientId,
                ["client_secret"] = _oauthSettings.ClientSecret
            };

            // 2. Call Token Endpoint
            var response = await _httpClient.PostAsync(
                _oauthSettings.TokenEndpoint,
                new FormUrlEncodedContent(requestBody));

            var content = await response.Content.ReadAsStringAsync();

            // 3. Parse response
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to refresh token. Status: {Status}, Response: {Response}",
                    response.StatusCode,
                    content);

                throw new Exception($"Token refresh failed: {response.StatusCode}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Invalid refresh token response: {Response}", content);
                throw new Exception("Invalid refresh token response from Keycloak");
            }

            _logger.LogInformation("Successfully refreshed access token");

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    /// <summary>
    /// Revoke token (logout)
    /// Best practice: revoke refresh token khi logout
    /// </summary>
    public async Task RevokeTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Revoking refresh token");

            // Keycloak revocation endpoint
            var revokeEndpoint = _oauthSettings.TokenEndpoint.Replace("/token", "/revoke");

            // Prepare request
            var requestBody = new Dictionary<string, string>
            {
                ["token"] = refreshToken,
                ["token_type_hint"] = "refresh_token",
                ["client_id"] = _oauthSettings.ClientId,
                ["client_secret"] = _oauthSettings.ClientSecret
            };

            var response = await _httpClient.PostAsync(
                revokeEndpoint,
                new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to revoke token. Status: {Status}, Response: {Response}",
                    response.StatusCode,
                    content);
            }
            else
            {
                _logger.LogInformation("Successfully revoked refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            // Không throw vì revoke failure không nên block logout flow
        }
    }

    /// <summary>
    /// Build Authorization URL để redirect browser tới Keycloak
    /// </summary>
    public string BuildAuthorizationUrl(Models.PkceData pkceData, string redirectUri)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["response_type"] = _oauthSettings.ResponseType,
            ["client_id"] = _oauthSettings.ClientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = string.Join(" ", _oauthSettings.Scopes),
            ["state"] = pkceData.State,
            ["code_challenge"] = pkceData.CodeChallenge,
            ["code_challenge_method"] = pkceData.CodeChallengeMethod
        };

        return QueryHelpers.AddQueryString(_oauthSettings.AuthorizationEndpoint, queryParams);
    }
}

