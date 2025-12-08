using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// PKCE Service interface
/// </summary>
public interface IPkceService
{
    /// <summary>
    /// Tạo PKCE data và lưu vào Redis
    /// </summary>
    Task<PkceData> GeneratePkceAsync(string redirectUri);

    /// <summary>
    /// Lấy và xóa PKCE data (one-time use)
    /// </summary>
    Task<PkceData?> GetAndRemovePkceAsync(string state);

    /// <summary>
    /// Tạo code verifier
    /// </summary>
    string GenerateCodeVerifier();

    /// <summary>
    /// Tạo code challenge từ verifier
    /// </summary>
    string GenerateCodeChallenge(string codeVerifier);

    /// <summary>
    /// Tạo random state
    /// </summary>
    string GenerateState();
}
