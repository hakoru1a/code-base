namespace ApiGateway.Models;

/// <summary>
/// Represents user session được lưu trong Redis
/// Session này map session_id với tokens và user info
/// </summary>
public class UserSession
{
    /// <summary>
    /// Session ID - unique identifier
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Access Token từ Keycloak
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token từ Keycloak
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// ID Token (OpenID Connect)
    /// </summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (thường là "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Thời điểm access token hết hạn (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Thời điểm session được tạo (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm session được access lần cuối (UTC)
    /// </summary>
    public DateTime LastAccessedAt { get; set; }

    /// <summary>
    /// Thời điểm session_id được rotate lần cuối (UTC)
    /// </summary>
    public DateTime? LastRotatedAt { get; set; }

    /// <summary>
    /// User ID từ token
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Username từ token
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email từ token
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User roles từ token
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Additional claims từ token
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();

    /// <summary>
    /// Client fingerprint để bind session với specific client
    /// </summary>
    public string ClientFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// IP Address khi tạo session
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent khi tạo session
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Kiểm tra access token đã hết hạn chưa
    /// </summary>
    public bool IsAccessTokenExpired()
    {
        return DateTime.UtcNow.AddSeconds(60) >= ExpiresAt;
    }

    /// <summary>
    /// Kiểm tra có cần refresh token không
    /// </summary>
    public bool NeedsRefresh(int bufferSeconds = 60)
    {
        return DateTime.UtcNow.AddSeconds(bufferSeconds) >= ExpiresAt;
    }

    /// <summary>
    /// Lấy session timeout dựa trên roles
    /// Admin: 2h, Manager: 4h, User: 8h
    /// </summary>
    public TimeSpan GetSessionTimeout()
    {
        if (Roles.Contains("admin", StringComparer.OrdinalIgnoreCase))
            return TimeSpan.FromHours(2);
        
        if (Roles.Any(r => r.Contains("manager", StringComparison.OrdinalIgnoreCase)))
            return TimeSpan.FromHours(4);
        
        return TimeSpan.FromHours(8);
    }
}
