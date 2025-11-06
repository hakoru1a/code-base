using ApiGateway.Configurations;
using ApiGateway.Models;
using Contracts.Common.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của Session Manager
/// Quản lý user sessions với Redis sử dụng IRedisRepository có sẵn từ Infrastructure
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly IRedisRepository _redisRepo;
    private readonly BffSettings _bffSettings;
    private readonly OAuthSettings _oauthSettings;
    private readonly ILogger<SessionManager> _logger;

    // Key prefix cho session trong Redis
    private const string SessionKeyPrefix = "session:";

    public SessionManager(
        IRedisRepository redisRepo,
        BffSettings bffSettings,
        OAuthSettings oauthSettings,
        ILogger<SessionManager> logger)
    {
        _redisRepo = redisRepo;
        _bffSettings = bffSettings;
        _oauthSettings = oauthSettings;
        _logger = logger;
    }

    /// <summary>
    /// Tạo session mới từ token response
    /// </summary>
    public async Task<string> CreateSessionAsync(TokenResponse tokenResponse)
    {
        try
        {
            // 1. Tạo random session ID
            var sessionId = GenerateSessionId();

            // 2. Parse access token để lấy user claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenResponse.AccessToken);

            // 3. Extract user info từ JWT claims
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "";
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "";
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

            // 4. Extract roles
            var roles = ExtractRoles(jwtToken);

            // 5. Extract additional claims
            // Group by Type để handle duplicate claims (ví dụ: allowed-origins có nhiều giá trị)
            // Lấy giá trị đầu tiên cho mỗi claim type
            var claims = jwtToken.Claims
                .Where(c => c.Type != "sub" && c.Type != "preferred_username" && c.Type != "email")
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.First().Value);

            // 6. Tạo UserSession object
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

            // 7. Lưu vào Redis
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

    /// <summary>
    /// Lấy session từ Redis dùng IRedisRepository
    /// </summary>
    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_bffSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            var session = await _redisRepo.GetAsync<UserSession>(cacheKey);

            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return null;
            }

            // Update last accessed time (sliding expiration)
            await UpdateLastAccessedAsync(sessionId);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session: {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// Update session (sau khi refresh token)
    /// </summary>
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

    /// <summary>
    /// Xóa session (logout) dùng IRedisRepository
    /// </summary>
    public async Task RemoveSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_bffSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            await _redisRepo.DeleteAsync(cacheKey);

            _logger.LogInformation("Session removed: {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Update last accessed time cho sliding expiration
    /// </summary>
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
            // Không throw để không break request flow
        }
    }

    /// <summary>
    /// Kiểm tra session validity dùng IRedisRepository
    /// </summary>
    public async Task<bool> IsValidSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_bffSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.ExistsAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check session validity: {SessionId}", sessionId);
            return false;
        }
    }

    #region Private Methods

    /// <summary>
    /// Lưu session vào Redis với expiration dùng IRedisRepository
    /// Note: IRedisRepository không support sliding expiration, chỉ dùng absolute
    /// Sliding expiration được implement thông qua UpdateLastAccessedAsync
    /// </summary>
    private async Task SaveSessionToRedisAsync(UserSession session)
    {
        var cacheKey = $"{_bffSettings.InstanceName}{SessionKeyPrefix}{session.SessionId}";

        // Dùng absolute expiration (Redis TTL)
        // Sliding expiration được handle bằng cách update lại TTL mỗi lần access
        var expiry = TimeSpan.FromMinutes(_bffSettings.SessionAbsoluteExpirationMinutes);

        await _redisRepo.SetAsync(cacheKey, session, expiry);
    }

    /// <summary>
    /// Lấy session mà không update last accessed
    /// (dùng internally để tránh infinite loop)
    /// </summary>
    private async Task<UserSession?> GetSessionWithoutUpdateAsync(string sessionId)
    {
        try
        {
            var cacheKey = $"{_bffSettings.InstanceName}{SessionKeyPrefix}{sessionId}";
            return await _redisRepo.GetAsync<UserSession>(cacheKey);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generate random session ID (32 bytes = 256 bits)
    /// </summary>
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

    /// <summary>
    /// Extract roles từ JWT token
    /// Keycloak có 2 loại roles: realm_access và resource_access
    /// </summary>
    private static List<string> ExtractRoles(JwtSecurityToken token)
    {
        var roles = new List<string>();

        // Realm roles
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
            catch { /* Ignore parse errors */ }
        }

        // Resource/Client roles
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
            catch { /* Ignore parse errors */ }
        }

        return roles.Distinct().ToList();
    }

    #endregion
}

