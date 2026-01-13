using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Identity;

/// <summary>
/// Service để cache JWT claims để giảm overhead parsing
/// </summary>
public interface IJwtClaimsCache
{
    /// <summary>
    /// Lấy hoặc tạo ClaimsPrincipal từ JWT token với caching
    /// </summary>
    Task<ClaimsPrincipal> GetOrCreateClaimsAsync(string token);

    /// <summary>
    /// Clear cache entry for specific token
    /// </summary>
    void ClearTokenCache(string token);

    /// <summary>
    /// Check if token is near expiration without parsing
    /// </summary>
    Task<bool> IsTokenNearExpirationAsync(string token, int bufferSeconds = 60);
}

public class JwtClaimsCache : IJwtClaimsCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<JwtClaimsCache> _logger;
    private readonly JwtSecurityTokenHandler _jwtHandler;

    public JwtClaimsCache(IMemoryCache cache, ILogger<JwtClaimsCache> logger)
    {
        _cache = cache;
        _logger = logger;
        _jwtHandler = new JwtSecurityTokenHandler();
    }

    public async Task<ClaimsPrincipal> GetOrCreateClaimsAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return new ClaimsPrincipal();

        try
        {
            // Use token signature as cache key (last part of JWT)
            var tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
                return await ParseJwtClaimsDirectly(token);

            var signature = tokenParts[2];
            var cacheKey = $"jwt_claims:{ComputeTokenHash(signature)}";

            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                var jwt = _jwtHandler.ReadJwtToken(token);

                // Set cache expiration to token expiration (but max 10 minutes)
                var tokenExpiry = jwt.ValidTo;
                var cacheExpiry = DateTime.UtcNow.AddMinutes(10);
                entry.AbsoluteExpiration = tokenExpiry < cacheExpiry ? tokenExpiry : cacheExpiry;

                // Set priority based on token type
                entry.Priority = CacheItemPriority.High;

                _logger.LogDebug("Caching JWT claims for signature: {SignaturePrefix}", signature.Substring(0, Math.Min(8, signature.Length)));

                return CreateClaimsPrincipal(jwt);
            });

            return result ?? new ClaimsPrincipal();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache JWT claims, parsing directly");
            return await ParseJwtClaimsDirectly(token);
        }
    }

    public void ClearTokenCache(string token)
    {
        try
        {
            var tokenParts = token.Split('.');
            if (tokenParts.Length == 3)
            {
                var signature = tokenParts[2];
                var cacheKey = $"jwt_claims:{ComputeTokenHash(signature)}";
                _cache.Remove(cacheKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear token cache");
        }
    }

    public async Task<bool> IsTokenNearExpirationAsync(string token, int bufferSeconds = 60)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        try
        {
            var cacheKey = $"token_expiry:{ComputeTokenHash(token)}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                var jwt = _jwtHandler.ReadJwtToken(token);
                var expiresAt = jwt.ValidTo;
                var nearExpiration = expiresAt <= DateTime.UtcNow.AddSeconds(bufferSeconds);

                // Cache for 1 minute
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                entry.Priority = CacheItemPriority.Normal;

                return nearExpiration;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check token expiration, assuming expired");
            return true; // Fail safe - assume token is expired
        }
    }

    private async Task<ClaimsPrincipal> ParseJwtClaimsDirectly(string token)
    {
        return await Task.Run(() =>
        {
            try
            {
                var jwt = _jwtHandler.ReadJwtToken(token);
                return CreateClaimsPrincipal(jwt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse JWT token directly");
                return new ClaimsPrincipal();
            }
        });
    }

    private ClaimsPrincipal CreateClaimsPrincipal(JwtSecurityToken jwt)
    {
        var claims = new List<Claim>();

        // Extract standard claims
        if (!string.IsNullOrEmpty(jwt.Subject))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, jwt.Subject));

        // Extract common claims
        foreach (var claim in jwt.Claims)
        {
            switch (claim.Type)
            {
                case "preferred_username":
                    claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    break;
                case "email":
                    claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                    break;
                case "given_name":
                    claims.Add(new Claim(ClaimTypes.GivenName, claim.Value));
                    break;
                case "family_name":
                    claims.Add(new Claim(ClaimTypes.Surname, claim.Value));
                    break;
                default:
                    claims.Add(claim);
                    break;
            }
        }

        // Extract roles from realm_access
        var realmAccess = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccess))
        {
            try
            {
                var realmData = JsonSerializer.Deserialize<RealmAccess>(realmAccess);
                if (realmData?.Roles != null)
                {
                    foreach (var role in realmData.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse realm_access roles");
            }
        }

        // Extract permissions
        var permissions = jwt.Claims.FirstOrDefault(c => c.Type == "permissions")?.Value;
        if (!string.IsNullOrEmpty(permissions))
        {
            foreach (var permission in permissions.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
    }

    private static string ComputeTokenHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    private class RealmAccess
    {
        public List<string>? Roles { get; set; }
    }
}