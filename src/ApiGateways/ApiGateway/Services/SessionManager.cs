using ApiGateway.Configurations;
using ApiGateway.Models;
using Contracts.Common.Interface;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của Session Manager
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly IRedisRepository _redisRepo;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<SessionManager> _logger;
    private readonly IClientFingerprintService _fingerprintService;

    private const string SessionKeyPrefix = "session:";
    private const string WillRemoveKeyPrefix = "will_remove:";
    private const string InvalidSessionKeyPrefix = "invalid_session:";

    public SessionManager(
        IRedisRepository redisRepo,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<SessionManager> logger,
        IClientFingerprintService fingerprintService)
    {
        _redisRepo = redisRepo;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
        _fingerprintService = fingerprintService;
    }

    public async Task<string> CreateSessionAsync(TokenResponse tokenResponse)
    {
        // Backward compatibility - create session without HTTP context
        return await CreateSessionInternalAsync(tokenResponse, null);
    }

    public async Task<string> CreateSessionAsync(TokenResponse tokenResponse, HttpContext httpContext)
    {
        return await CreateSessionInternalAsync(tokenResponse, httpContext);
    }

    private async Task<string> CreateSessionInternalAsync(TokenResponse tokenResponse, HttpContext? httpContext)
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
                Claims = claims,
                // Enhanced security features
                ClientFingerprint = httpContext != null ? _fingerprintService.GenerateFingerprint(httpContext) : "",
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? ""
            };

            await SaveSessionToRedisAsync(session);

            _logger.LogInformation(
                "Session created for user {Username} (ID: {UserId}), SessionId: {SessionId}, IP: {IpAddress}",
                username,
                userId,
                sessionId,
                session.IpAddress);

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
            // Check if session is marked as invalid first
            var invalidKey = $"{_oauthOptions.InstanceName}{InvalidSessionKeyPrefix}{sessionId}";
            if (await _redisRepo.ExistsAsync(invalidKey))
            {
                _logger.LogWarning("Attempted to access invalidated session: {SessionId}", sessionId);
                return null;
            }

            var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{sessionId}";
            var session = await _redisRepo.GetAsync<UserSession>(cacheKey);

            if (session == null)
            {
                // Check xem session_id có trong will_remove list không (đã bị rotate)
                var newSessionId = await GetNewSessionIdFromWillRemoveListAsync(sessionId);
                if (!string.IsNullOrEmpty(newSessionId))
                {
                    _logger.LogInformation(
                        "Session ID redirected from will_remove list: {OldSessionId} -> {NewSessionId}",
                        sessionId,
                        newSessionId);
                    return await GetSessionAsync(newSessionId);
                }

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
            var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{sessionId}";
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
            var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.ExistsAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check session validity: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<string> RotateSessionIdAsync(string oldSessionId)
    {
        try
        {
            // Lấy session cũ
            var oldSession = await GetSessionWithoutUpdateAsync(oldSessionId);
            if (oldSession == null)
            {
                _logger.LogWarning("Cannot rotate: Session not found: {SessionId}", oldSessionId);
                throw new InvalidOperationException("Session not found");
            }

            // Tạo session_id mới
            var newSessionId = GenerateSessionId();

            // Tạo session mới với tất cả data từ session cũ
            var newSession = new UserSession
            {
                SessionId = newSessionId,
                AccessToken = oldSession.AccessToken,
                RefreshToken = oldSession.RefreshToken,
                IdToken = oldSession.IdToken,
                TokenType = oldSession.TokenType,
                ExpiresAt = oldSession.ExpiresAt,
                CreatedAt = oldSession.CreatedAt,
                LastAccessedAt = DateTime.UtcNow,
                LastRotatedAt = DateTime.UtcNow,
                UserId = oldSession.UserId,
                Username = oldSession.Username,
                Email = oldSession.Email,
                Roles = oldSession.Roles,
                Claims = oldSession.Claims
            };

            // Lưu session mới
            await SaveSessionToRedisAsync(newSession);

            // Đưa session_id cũ vào will_remove list với TTL 24h
            var willRemoveKey = $"{_oauthOptions.InstanceName}{WillRemoveKeyPrefix}{oldSessionId}";
            await _redisRepo.SetAsync(willRemoveKey, newSessionId, TimeSpan.FromHours(24));

            // Xóa session cũ khỏi Redis
            var oldCacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{oldSessionId}";
            await _redisRepo.DeleteAsync(oldCacheKey);

            _logger.LogInformation(
                "Session rotated for user {Username}, Old SessionId: {OldSessionId}, New SessionId: {NewSessionId}",
                oldSession.Username,
                oldSessionId,
                newSessionId);

            return newSessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate session: {SessionId}", oldSessionId);
            throw;
        }
    }

    #region Private Methods

    public async Task InvalidateSessionAsync(string sessionId)
    {
        try
        {
            // Mark session as invalid immediately
            var invalidKey = $"{_oauthOptions.InstanceName}{InvalidSessionKeyPrefix}{sessionId}";
            await _redisRepo.SetAsync(invalidKey, true, TimeSpan.FromHours(24));

            // Remove session data
            var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{sessionId}";
            await _redisRepo.DeleteAsync(cacheKey);

            _logger.LogWarning("Session invalidated immediately: {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> ValidateSessionContextAsync(string sessionId, HttpContext httpContext)
    {
        try
        {
            var session = await GetSessionWithoutUpdateAsync(sessionId);
            if (session == null)
                return false;

            // Validate client fingerprint if available
            if (!string.IsNullOrEmpty(session.ClientFingerprint))
            {
                if (!_fingerprintService.ValidateFingerprint(session.ClientFingerprint, httpContext))
                {
                    _logger.LogWarning(
                        "Client fingerprint mismatch for session {SessionId}, User: {Username}",
                        sessionId,
                        session.Username);
                    
                    // Invalidate session on fingerprint mismatch
                    await InvalidateSessionAsync(sessionId);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate session context: {SessionId}", sessionId);
            return false;
        }
    }

    private async Task SaveSessionToRedisAsync(UserSession session)
    {
        var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{session.SessionId}";
        
        // Use role-based timeout instead of fixed timeout
        var expiry = session.GetSessionTimeout();
        
        // Ensure we don't exceed configured maximum
        var maxExpiry = TimeSpan.FromMinutes(_oauthOptions.SessionAbsoluteExpirationMinutes);
        if (expiry > maxExpiry)
            expiry = maxExpiry;
            
        await _redisRepo.SetAsync(cacheKey, session, expiry);
    }

    private async Task<UserSession?> GetSessionWithoutUpdateAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_oauthOptions.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.GetAsync<UserSession>(cacheKey);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetNewSessionIdFromWillRemoveListAsync(string oldSessionId)
    {
        try
        {
            var willRemoveKey = $"{_oauthOptions.InstanceName}{WillRemoveKeyPrefix}{oldSessionId}";
            return await _redisRepo.GetAsync<string>(willRemoveKey);
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
