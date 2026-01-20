using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models;

/// <summary>
/// Response trả về sau khi login thành công - JWT approach
/// </summary>
public class AuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }
}

/// <summary>
/// Request để refresh access token
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request để logout và revoke tokens
/// </summary>
public class LogoutRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}

/// <summary>
/// User claims được cache trong Redis thay thế session
/// Extracted từ JWT ID token
/// </summary>
public class CachedUserClaims
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonPropertyName("claims")]
    public Dictionary<string, string> Claims { get; set; } = new();

    [JsonPropertyName("cached_at")]
    public DateTime CachedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// ID token để dùng cho logout (end session ở Keycloak)
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    /// <summary>
    /// Kiểm tra cache đã hết hạn chưa
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// Temporary token code data - Lưu token tạm thời trong Redis
/// Dùng cho Temporary Code Exchange pattern (an toàn hơn token trên URL)
/// </summary>
public class TemporaryTokenCode
{
    /// <summary>
    /// Temporary code - random string để exchange lấy token
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Access token từ Keycloak
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token từ Keycloak
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token type (thường là "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Số giây access token còn hiệu lực
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Redirect URL về frontend
    /// </summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Thời điểm tạo code (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm code hết hạn (UTC) - thường 2 phút
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Kiểm tra code đã hết hạn chưa
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// Request để exchange temporary code lấy token
/// </summary>
public class ExchangeTokenRequest
{
    [Required(ErrorMessage = "Code is required")]
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// User profile response - Thông tin profile user được format đẹp
/// </summary>
public class UserProfileResponse
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("last_login")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("profile_updated_at")]
    public DateTime? ProfileUpdatedAt { get; set; }

    /// <summary>
    /// Convert từ CachedUserClaims sang UserProfileResponse
    /// </summary>
    public static UserProfileResponse FromCachedUserClaims(CachedUserClaims userClaims)
    {
        var profile = new UserProfileResponse
        {
            UserId = userClaims.UserId,
            Username = userClaims.Username,
            Email = userClaims.Email,
            Roles = userClaims.Roles,
            LastLogin = userClaims.CachedAt
        };

        // Extract additional info từ claims
        if (userClaims.Claims.TryGetValue("name", out var fullName))
            profile.FullName = fullName;

        if (userClaims.Claims.TryGetValue("given_name", out var firstName))
            profile.FirstName = firstName;

        if (userClaims.Claims.TryGetValue("family_name", out var lastName))
            profile.LastName = lastName;

        if (userClaims.Claims.TryGetValue("picture", out var picture))
            profile.Picture = picture;

        if (userClaims.Claims.TryGetValue("email_verified", out var emailVerifiedStr))
            profile.EmailVerified = bool.TryParse(emailVerifiedStr, out var emailVerified) && emailVerified;

        if (userClaims.Claims.TryGetValue("locale", out var locale))
            profile.Locale = locale;

        if (userClaims.Claims.TryGetValue("zoneinfo", out var timezone))
            profile.Timezone = timezone;

        if (userClaims.Claims.TryGetValue("updated_at", out var updatedAtStr))
        {
            if (long.TryParse(updatedAtStr, out var updatedAtTimestamp))
                profile.ProfileUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(updatedAtTimestamp).DateTime;
        }

        return profile;
    }
}