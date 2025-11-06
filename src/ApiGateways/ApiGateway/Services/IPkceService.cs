using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// Interface cho PKCE (Proof Key for Code Exchange) service
/// PKCE là security extension cho OAuth 2.0 Authorization Code Flow
/// </summary>
public interface IPkceService
{
    /// <summary>
    /// Tạo mới PKCE data (code_verifier, code_challenge, state)
    /// và lưu vào Redis với state làm key
    /// </summary>
    /// <param name="redirectUri">URI để redirect sau khi login thành công</param>
    /// <returns>PKCE data với code_challenge để gửi lên Keycloak</returns>
    Task<PkceData> GeneratePkceAsync(string redirectUri);

    /// <summary>
    /// Lấy và xóa PKCE data từ Redis bằng state
    /// (Chỉ được dùng 1 lần - one-time use)
    /// </summary>
    /// <param name="state">State parameter từ callback</param>
    /// <returns>PKCE data nếu tồn tại và chưa expire, null nếu không</returns>
    Task<PkceData?> GetAndRemovePkceAsync(string state);

    /// <summary>
    /// Tạo random code verifier theo OAuth 2.0 PKCE spec
    /// Length: 43-128 characters
    /// Characters: [A-Z] [a-z] [0-9] - . _ ~
    /// </summary>
    string GenerateCodeVerifier();

    /// <summary>
    /// Tạo code challenge từ code verifier
    /// Method: S256 (SHA256 hash, base64url encoded)
    /// </summary>
    string GenerateCodeChallenge(string codeVerifier);

    /// <summary>
    /// Tạo random state cho CSRF protection
    /// </summary>
    string GenerateState();
}

