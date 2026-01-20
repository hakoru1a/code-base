using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// Service để quản lý temporary token codes
/// Dùng cho Temporary Code Exchange pattern (an toàn hơn token trên URL)
/// </summary>
public interface ITemporaryTokenService
{
    /// <summary>
    /// Tạo temporary code và lưu token data vào Redis
    /// </summary>
    /// <param name="tokenResponse">Token response từ Keycloak</param>
    /// <param name="redirectUrl">URL redirect về frontend</param>
    /// <returns>Temporary code để gửi về frontend</returns>
    Task<string> CreateTemporaryCodeAsync(TokenResponse tokenResponse, string? redirectUrl = null);

    /// <summary>
    /// Lấy và xóa token data bằng temporary code (one-time use)
    /// </summary>
    /// <param name="code">Temporary code từ frontend</param>
    /// <returns>Token data hoặc null nếu code không hợp lệ/hết hạn</returns>
    Task<TemporaryTokenCode?> GetAndRemoveTokenAsync(string code);

    /// <summary>
    /// Tạo random temporary code
    /// </summary>
    /// <returns>Random code string</returns>
    string GenerateTemporaryCode();
}