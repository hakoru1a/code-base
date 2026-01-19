using ApiGateway.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của IUserClaimsCache sử dụng Redis distributed cache
/// </summary>
public class UserClaimsCache : IUserClaimsCache
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<UserClaimsCache> _logger;
    private const string CacheKeyPrefix = "user_claims:";

    public UserClaimsCache(
        IDistributedCache distributedCache,
        ILogger<UserClaimsCache> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task CacheUserClaimsAsync(string idToken, int expirationMinutes = 60)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(idToken);

            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value 
                ?? throw new InvalidOperationException("User ID not found in token");

            var userClaims = new CachedUserClaims
            {
                UserId = userId,
                Username = jsonToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "",
                Email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                Roles = ExtractRoles(jsonToken.Claims),
                Claims = ExtractClaims(jsonToken.Claims),
                CachedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var cacheKey = GetCacheKey(userId);
            var cacheValue = JsonSerializer.Serialize(userClaims);
            
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(expirationMinutes),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes * 2)
            };

            await _distributedCache.SetStringAsync(cacheKey, cacheValue, cacheOptions);

            _logger.LogInformation("User claims cached successfully for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching user claims from ID token");
            throw;
        }
    }

    public async Task<CachedUserClaims?> GetUserClaimsAsync(string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            var cachedValue = await _distributedCache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("No cached claims found for user: {UserId}", userId);
                return null;
            }

            var userClaims = JsonSerializer.Deserialize<CachedUserClaims>(cachedValue);

            if (userClaims?.IsExpired() == true)
            {
                _logger.LogDebug("Cached claims expired for user: {UserId}", userId);
                await _distributedCache.RemoveAsync(cacheKey);
                return null;
            }

            _logger.LogDebug("Retrieved cached claims for user: {UserId}", userId);
            return userClaims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user claims for user: {UserId}", userId);
            return null;
        }
    }

    public async Task RemoveUserClaimsAsync(string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            await _distributedCache.RemoveAsync(cacheKey);
            _logger.LogInformation("User claims removed from cache for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user claims for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsUserClaimsValidAsync(string userId)
    {
        var userClaims = await GetUserClaimsAsync(userId);
        return userClaims != null && !userClaims.IsExpired();
    }

    private static string GetCacheKey(string userId) => $"{CacheKeyPrefix}{userId}";

    private static List<string> ExtractRoles(IEnumerable<Claim> claims)
    {
        var roles = new List<string>();

        // Keycloak realm roles
        var realmRoles = claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmRoles))
        {
            try
            {
                var realmAccess = JsonDocument.Parse(realmRoles);
                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                {
                    roles.AddRange(rolesElement.EnumerateArray().Select(r => r.GetString() ?? ""));
                }
            }
            catch (Exception)
            {
                // Ignore parsing errors
            }
        }

        // Standard role claims
        roles.AddRange(claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                           .Select(c => c.Value));

        return roles.Distinct().Where(r => !string.IsNullOrEmpty(r)).ToList();
    }

    private static Dictionary<string, string> ExtractClaims(IEnumerable<Claim> claims)
    {
        var claimsDict = new Dictionary<string, string>();

        foreach (var claim in claims)
        {
            // Skip sensitive or redundant claims
            if (ShouldIncludeClaim(claim.Type))
            {
                claimsDict[claim.Type] = claim.Value;
            }
        }

        return claimsDict;
    }

    private static bool ShouldIncludeClaim(string claimType)
    {
        // Include important claims but exclude sensitive ones
        var includedClaims = new[]
        {
            "name", "given_name", "family_name", "nickname",
            "picture", "locale", "zoneinfo", "updated_at",
            "email_verified", "phone_number", "phone_number_verified"
        };

        var excludedClaims = new[]
        {
            "sub", "iss", "aud", "exp", "nbf", "iat", "jti",
            "preferred_username", "email", "realm_access", "resource_access"
        };

        return includedClaims.Contains(claimType) && !excludedClaims.Contains(claimType);
    }
}