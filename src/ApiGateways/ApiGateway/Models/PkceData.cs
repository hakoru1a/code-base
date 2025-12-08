namespace ApiGateway.Models;

/// <summary>
/// PKCE (Proof Key for Code Exchange) data
/// PKCE là security extension cho OAuth 2.0 để chống code interception attack
/// </summary>
public class PkceData
{
    /// <summary>
    /// Code verifier - random string (43-128 chars)
    /// Được lưu trong Redis, gắn với state
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// Code challenge - SHA256 hash của code_verifier
    /// </summary>
    public string CodeChallenge { get; set; } = string.Empty;

    /// <summary>
    /// Code challenge method (S256 hoặc plain)
    /// </summary>
    public string CodeChallengeMethod { get; set; } = "S256";

    /// <summary>
    /// State parameter - random string for CSRF protection
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI sau khi login
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm tạo PKCE data (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm PKCE data hết hạn (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
