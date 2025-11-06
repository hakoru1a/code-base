namespace ApiGateway.Models;

/// <summary>
/// PKCE (Proof Key for Code Exchange) data
/// PKCE là security extension cho OAuth 2.0 để chống code interception attack
/// Flow:
/// 1. Tạo random code_verifier
/// 2. Hash code_verifier thành code_challenge
/// 3. Gửi code_challenge lên Keycloak khi request authorization
/// 4. Keycloak trả về authorization code
/// 5. Gửi authorization code + code_verifier để đổi lấy token
/// 6. Keycloak verify code_challenge = hash(code_verifier)
/// </summary>
public class PkceData
{
    /// <summary>
    /// Code verifier - random string (43-128 chars)
    /// Được lưu trong Redis, gắn với state
    /// KHÔNG BAO GIỜ gửi lên browser hay Keycloak trong phase 1
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// Code challenge - SHA256 hash của code_verifier
    /// Được gửi lên Keycloak trong authorization request
    /// </summary>
    public string CodeChallenge { get; set; } = string.Empty;

    /// <summary>
    /// Code challenge method (S256 hoặc plain)
    /// S256 = SHA256 hash (RECOMMENDED)
    /// plain = không hash (NOT RECOMMENDED, chỉ dùng nếu device không support crypto)
    /// </summary>
    public string CodeChallengeMethod { get; set; } = "S256";

    /// <summary>
    /// State parameter - random string for CSRF protection
    /// Được gửi lên Keycloak và so sánh khi callback
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI sau khi login
    /// Lưu lại để redirect user đúng nơi
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm tạo PKCE data (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm PKCE data hết hạn (UTC)
    /// Thường là 10 phút sau khi tạo
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

