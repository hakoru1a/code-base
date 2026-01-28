using ApiGateway.Configurations;
using ApiGateway.Models;
using Contracts.Common.Interface;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Services;

/// <summary>
/// Implementation của Temporary Token Service
/// Quản lý temporary codes để exchange token an toàn
/// </summary>
public class TemporaryTokenService : ITemporaryTokenService
{
    private readonly IRedisRepository _redisRepo;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<TemporaryTokenService> _logger;

    private const string TempTokenKeyPrefix = "temp_token:";
    private const int DefaultExpirationMinutes = 2; // Code hết hạn sau 2 phút

    public TemporaryTokenService(
        IRedisRepository redisRepo,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<TemporaryTokenService> logger)
    {
        _redisRepo = redisRepo;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    public async Task<string> CreateTemporaryCodeAsync(TokenResponse tokenResponse, string? redirectUrl = null)
    {
        try
        {
            var code = GenerateTemporaryCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(DefaultExpirationMinutes);

            var tempTokenData = new TemporaryTokenCode
            {
                Code = code,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                RedirectUrl = redirectUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            var cacheKey = $"{_oauthOptions.InstanceName}{TempTokenKeyPrefix}{code}";
            var expiry = TimeSpan.FromMinutes(DefaultExpirationMinutes);

            await _redisRepo.SetAsync(cacheKey, tempTokenData, expiry);

            _logger.LogInformation(
                "Temporary token code created: {Code}, expires at: {ExpiresAt}",
                code,
                expiresAt);

            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create temporary token code");
            throw;
        }
    }

    public async Task<TemporaryTokenCode?> GetAndRemoveTokenAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Empty temporary code provided");
                return null;
            }

            var cacheKey = $"{_oauthOptions.InstanceName}{TempTokenKeyPrefix}{code}";

            var tempTokenData = await _redisRepo.GetAsync<TemporaryTokenCode>(cacheKey);

            if (tempTokenData == null)
            {
                _logger.LogWarning("Temporary token code not found: {Code}", code);
                return null;
            }

            if (tempTokenData.IsExpired())
            {
                _logger.LogWarning("Temporary token code expired: {Code}", code);
                await _redisRepo.DeleteAsync(cacheKey);
                return null;
            }

            // Xóa code sau khi sử dụng (one-time use)
            await _redisRepo.DeleteAsync(cacheKey);

            _logger.LogInformation("Temporary token code retrieved and removed: {Code}", code);

            return tempTokenData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get temporary token data for code: {Code}", code);
            throw;
        }
    }

    public string GenerateTemporaryCode()
    {
        // Tạo code 32 ký tự, base64url encoded
        const int length = 24; // 24 bytes = 32 chars when base64url encoded
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