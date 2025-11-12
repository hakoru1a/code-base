using Auth.Application.Interfaces;
using Auth.Domain.Configurations;
using Auth.Domain.Models;
using Contracts.Common.Interface;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Implementation của Session Manager
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly IRedisRepository _redisRepo;
    private readonly AuthSettings _authSettings;
    private readonly OAuthSettings _oauthSettings;
    private readonly ILogger<SessionManager> _logger;

    private const string SessionKeyPrefix = "session:";

    public SessionManager(
        IRedisRepository redisRepo,
        AuthSettings authSettings,
        OAuthSettings oauthSettings,
        ILogger<SessionManager> logger)
    {
        _redisRepo = redisRepo;
        _authSettings = authSettings;
        _oauthSettings = oauthSettings;
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(TokenResponse tokenResponse)
    {
        try
        {
            var sessionId = GenerateSessionId();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenResponse.AccessToken);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "";
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "";
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

            var roles = ExtractRoles(jwtToken);

            var claims = jwtToken.Claims
                .Where(c => c.Type != "sub" && c.Type != "preferred_username" && c.Type != "email")
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.First().Value);

            var session = new UserSession
            {
                SessionId = sessionId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken ?? "",
                IdToken = tokenResponse.IdToken ?? "",
                TokenType = tokenResponse.TokenType,
                ExpiresAt = tokenResponse.CalculateExpiresAt(),
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                UserId = userId,
                Username = username,
                Email = email,
                Roles = roles,
                Claims = claims
            };

            await SaveSessionToRedisAsync(session);

            _logger.LogInformation(
                "Session created for user {Username} (ID: {UserId}), SessionId: {SessionId}",
                username,
                userId,
                sessionId);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session");
            throw;
        }
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_authSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            var session = await _redisRepo.GetAsync<UserSession>(cacheKey);

            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return null;
            }

            await UpdateLastAccessedAsync(sessionId);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session: {SessionId}", sessionId);
            return null;
        }
    }

    public async Task UpdateSessionAsync(UserSession session)
    {
        try
        {
            session.LastAccessedAt = DateTime.UtcNow;
            await SaveSessionToRedisAsync(session);

            _logger.LogInformation(
                "Session updated for user {Username}, SessionId: {SessionId}",
                session.Username,
                session.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update session: {SessionId}", session.SessionId);
            throw;
        }
    }

    public async Task RemoveSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_authSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            await _redisRepo.DeleteAsync(cacheKey);

            _logger.LogInformation("Session removed: {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task UpdateLastAccessedAsync(string sessionId)
    {
        try
        {
            var session = await GetSessionWithoutUpdateAsync(sessionId);
            if (session == null) return;

            session.LastAccessedAt = DateTime.UtcNow;
            await SaveSessionToRedisAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update last accessed: {SessionId}", sessionId);
        }
    }

    public async Task<bool> IsValidSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_authSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.ExistsAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check session validity: {SessionId}", sessionId);
            return false;
        }
    }

    #region Private Methods

    private async Task SaveSessionToRedisAsync(UserSession session)
    {
        var cacheKey = $"{_authSettings.InstanceName}{SessionKeyPrefix}{session.SessionId}";
        var expiry = TimeSpan.FromMinutes(_authSettings.SessionAbsoluteExpirationMinutes);
        await _redisRepo.SetAsync(cacheKey, session, expiry);
    }

    private async Task<UserSession?> GetSessionWithoutUpdateAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_authSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.GetAsync<UserSession>(cacheKey);
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSessionId()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static List<string> ExtractRoles(JwtSecurityToken token)
    {
        var roles = new List<string>();

        var realmAccess = token.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccess))
        {
            try
            {
                var realmJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(realmAccess);
                if (realmJson != null && realmJson.TryGetValue("roles", out var rolesElement))
                {
                    var realmRoles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                    if (realmRoles != null)
                        roles.AddRange(realmRoles);
                }
            }
            catch { }
        }

        var resourceAccess = token.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
        if (!string.IsNullOrEmpty(resourceAccess))
        {
            try
            {
                var resourceJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resourceAccess);
                if (resourceJson != null)
                {
                    foreach (var resource in resourceJson.Values)
                    {
                        if (resource.TryGetProperty("roles", out var rolesElement))
                        {
                            var resourceRoles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                            if (resourceRoles != null)
                                roles.AddRange(resourceRoles);
                        }
                    }
                }
            }
            catch { }
        }

        return roles.Distinct().ToList();
    }

    #endregion
}
