using Auth.Application.Interfaces;
using Auth.Domain.Configurations;
using Auth.Domain.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Implementation của OAuth Client
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
                ["client_id"] = _oauthSettings.ClientId,
                ["client_secret"] = _oauthSettings.ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier
            };

            var response = await _httpClient.PostAsync(
                _oauthSettings.TokenEndpoint,
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
                ["client_id"] = _oauthSettings.ClientId,
                ["client_secret"] = _oauthSettings.ClientSecret
            };

            var response = await _httpClient.PostAsync(
                _oauthSettings.TokenEndpoint,
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

            var revokeEndpoint = _oauthSettings.TokenEndpoint.Replace("/token", "/revoke");

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
        }
    }

    public string BuildAuthorizationUrl(PkceData pkceData, string redirectUri)
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
