using System.Text.Json.Serialization;

namespace ApiGateway.Models;

/// <summary>
/// Response từ Keycloak Token Endpoint
/// Conform với OAuth 2.0 / OpenID Connect spec
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Access token để call APIs
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (thường là "Bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Số giây access token còn hiệu lực
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Refresh token để lấy access token mới
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Số giây refresh token còn hiệu lực
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; set; }

    /// <summary>
    /// ID token (OpenID Connect) chứa user info
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    /// <summary>
    /// Scopes được grant
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Session state (Keycloak specific)
    /// </summary>
    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }

    /// <summary>
    /// Error code nếu request failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Error description
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Tính toán thời điểm token sẽ hết hạn
    /// </summary>
    public DateTime CalculateExpiresAt()
    {
        return DateTime.UtcNow.AddSeconds(ExpiresIn);
    }
}

