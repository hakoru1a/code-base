using ApiGateway.Configurations;
using ApiGateway.Models;
using Contracts.Common.Interface;
using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của PKCE service
/// Handles PKCE flow security cho OAuth 2.0
/// Sử dụng IRedisRepository có sẵn từ Infrastructure
/// </summary>
public class PkceService : IPkceService
{
    private readonly IRedisRepository _redisRepo;
    private readonly BffSettings _bffSettings;
    private readonly ILogger<PkceService> _logger;

    // Key prefix cho PKCE data trong Redis
    private const string PkceKeyPrefix = "pkce:";

    public PkceService(
        IRedisRepository redisRepo,
        BffSettings bffSettings,
        ILogger<PkceService> logger)
    {
        _redisRepo = redisRepo;
        _bffSettings = bffSettings;
        _logger = logger;
    }

    /// <summary>
    /// Tạo PKCE data và lưu vào Redis
    /// </summary>
    public async Task<PkceData> GeneratePkceAsync(string redirectUri)
    {
        try
        {
            // 1. Tạo random code_verifier (độ dài 43-128 chars theo spec)
            var codeVerifier = GenerateCodeVerifier();

            // 2. Hash code_verifier thành code_challenge
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            // 3. Tạo random state cho CSRF protection
            var state = GenerateState();

            // 4. Tạo PKCE data object
            var pkceData = new PkceData
            {
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = "S256",
                State = state,
                RedirectUri = redirectUri,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_bffSettings.PkceExpirationMinutes)
            };

            // 5. Lưu vào Redis với key = "pkce:{state}" dùng IRedisRepository
            var cacheKey = $"{_bffSettings.InstanceName}{PkceKeyPrefix}{state}";
            var expiry = TimeSpan.FromMinutes(_bffSettings.PkceExpirationMinutes);

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

    /// <summary>
    /// Lấy và xóa PKCE data (one-time use)
    /// </summary>
    public async Task<PkceData?> GetAndRemovePkceAsync(string state)
    {
        try
        {
            var cacheKey = $"{_bffSettings.InstanceName}{PkceKeyPrefix}{state}";

            // 1. Lấy data từ Redis dùng IRedisRepository
            var pkceData = await _redisRepo.GetAsync<PkceData>(cacheKey);

            if (pkceData == null)
            {
                _logger.LogWarning("PKCE data not found for state: {State}", state);
                return null;
            }

            // 2. Kiểm tra expiration
            if (DateTime.UtcNow > pkceData.ExpiresAt)
            {
                _logger.LogWarning("PKCE data expired for state: {State}", state);
                await _redisRepo.DeleteAsync(cacheKey);
                return null;
            }

            // 3. Xóa khỏi Redis (one-time use để prevent replay attack)
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

    /// <summary>
    /// Tạo code verifier theo spec:
    /// - Length: 43-128 characters (chọn 64 cho balanced security)
    /// - Characters: [A-Z] [a-z] [0-9] - . _ ~
    /// </summary>
    public string GenerateCodeVerifier()
    {
        const int length = 64; // Balanced length (spec allows 43-128)
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

    /// <summary>
    /// Tạo code challenge từ verifier
    /// Method: S256 = BASE64URL(SHA256(ASCII(code_verifier)))
    /// </summary>
    public string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));

        // Base64Url encode (không có padding và replace chars)
        return Base64UrlEncode(hash);
    }

    /// <summary>
    /// Tạo random state (32 chars)
    /// </summary>
    public string GenerateState()
    {
        const int length = 32;
        var bytes = new byte[length];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Base64Url encoding (RFC 4648 Section 5)
    /// Khác Base64 thường:
    /// - Không có padding (=)
    /// - Replace + với -
    /// - Replace / với _
    /// </summary>
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

