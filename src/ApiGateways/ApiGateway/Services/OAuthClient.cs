using ApiGateway.Configurations;
using ApiGateway.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của OAuth Client
/// </summary>
public class OAuthClient : IOAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<OAuthClient> _logger;

    public OAuthClient(
        HttpClient httpClient,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<OAuthClient> logger)
    {
        _httpClient = httpClient;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    public async Task<TokenResponse> ExchangeCodeForTokensAsync(
        string code,
        string codeVerifier,
        string redirectUri)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for tokens");

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = _oauthOptions.ClientId,
                ["client_secret"] = _oauthOptions.ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier
            };

            var response = await _httpClient.PostAsync(
                _oauthOptions.TokenEndpoint,
                new FormUrlEncodedContent(requestBody));

            var content = await response.Content.ReadAsStringAsync();

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

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Refreshing access token");

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _oauthOptions.ClientId,
                ["client_secret"] = _oauthOptions.ClientSecret
            };

            var response = await _httpClient.PostAsync(
                _oauthOptions.TokenEndpoint,
                new FormUrlEncodedContent(requestBody));

            var content = await response.Content.ReadAsStringAsync();

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

    public async Task RevokeTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Revoking refresh token");

            var revokeEndpoint = _oauthOptions.TokenEndpoint.Replace("/token", "/revoke");

            var requestBody = new Dictionary<string, string>
            {
                ["token"] = refreshToken,
                ["token_type_hint"] = "refresh_token",
                ["client_id"] = _oauthOptions.ClientId,
                ["client_secret"] = _oauthOptions.ClientSecret
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
        }
    }

    public async Task EndSessionAsync(string? idToken = null, string? postLogoutRedirectUri = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_oauthOptions.EndSessionEndpoint))
            {
                _logger.LogWarning("End session endpoint is not configured, skipping end session call");
                return;
            }

            _logger.LogInformation("Ending session at Keycloak");

            var queryParams = new Dictionary<string, string?>
            {
                ["client_id"] = _oauthOptions.ClientId
            };

            // Thêm id_token_hint nếu có
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                queryParams["id_token_hint"] = idToken;
            }

            // Thêm post_logout_redirect_uri nếu có
            if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri))
            {
                queryParams["post_logout_redirect_uri"] = postLogoutRedirectUri;
            }
            else if (!string.IsNullOrWhiteSpace(_oauthOptions.PostLogoutRedirectUri))
            {
                queryParams["post_logout_redirect_uri"] = _oauthOptions.PostLogoutRedirectUri;
            }

            var endSessionUrl = QueryHelpers.AddQueryString(_oauthOptions.EndSessionEndpoint, queryParams);

            var response = await _httpClient.GetAsync(endSessionUrl);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to end session at Keycloak. Status: {Status}, Response: {Response}",
                    response.StatusCode,
                    content);
            }
            else
            {
                _logger.LogInformation("Successfully ended session at Keycloak");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session at Keycloak");
            // Không throw exception để không làm gián đoạn logout flow
        }
    }

    public string BuildAuthorizationUrl(PkceData pkceData, string redirectUri)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["response_type"] = _oauthOptions.ResponseType,
            ["client_id"] = _oauthOptions.ClientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = string.Join(" ", _oauthOptions.Scopes),
            ["state"] = pkceData.State,
            ["code_challenge"] = pkceData.CodeChallenge,
            ["code_challenge_method"] = pkceData.CodeChallengeMethod
        };

        return QueryHelpers.AddQueryString(_oauthOptions.AuthorizationEndpoint, queryParams);
    }
}
