using ApiGateway.Configurations;
using ApiGateway.Models;
using Contracts.Common.Interface;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của PKCE service
/// </summary>
public class PkceService : IPkceService
{
    private readonly IRedisRepository _redisRepo;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<PkceService> _logger;

    private const string PkceKeyPrefix = "pkce:";

    public PkceService(
        IRedisRepository redisRepo,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<PkceService> logger)
    {
        _redisRepo = redisRepo;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    public async Task<PkceData> GeneratePkceAsync(string redirectUri)
    {
        try
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var state = GenerateState();

            var pkceData = new PkceData
            {
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = "S256",
                State = state,
                RedirectUri = redirectUri,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_oauthOptions.PkceExpirationMinutes)
            };

            var cacheKey = $"{_oauthOptions.InstanceName}{PkceKeyPrefix}{state}";
            var expiry = TimeSpan.FromMinutes(_oauthOptions.PkceExpirationMinutes);

            await _redisRepo.SetAsync(cacheKey, pkceData, expiry);

            _logger.LogInformation(
                "PKCE data created with state: {State}, expires at: {ExpiresAt}",
                state,
                pkceData.ExpiresAt);

            return pkceData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PKCE data");
            throw;
        }
    }

    public async Task<PkceData?> GetAndRemovePkceAsync(string state)
    {
        try
        {
            var cacheKey = $"{_oauthOptions.InstanceName}{PkceKeyPrefix}{state}";

            var pkceData = await _redisRepo.GetAsync<PkceData>(cacheKey);

            if (pkceData == null)
            {
                _logger.LogWarning("PKCE data not found for state: {State}", state);
                return null;
            }

            if (DateTime.UtcNow > pkceData.ExpiresAt)
            {
                _logger.LogWarning("PKCE data expired for state: {State}", state);
                await _redisRepo.DeleteAsync(cacheKey);
                return null;
            }

            await _redisRepo.DeleteAsync(cacheKey);

            _logger.LogInformation("PKCE data retrieved and removed for state: {State}", state);

            return pkceData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PKCE data for state: {State}", state);
            throw;
        }
    }

    public string GenerateCodeVerifier()
    {
        const int length = 64;
        const string unreservedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";

        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            result.Append(unreservedChars[b % unreservedChars.Length]);
        }

        return result.ToString();
    }

    public string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    public string GenerateState()
    {
        const int length = 32;
        var bytes = new byte[length];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
