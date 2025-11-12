namespace Auth.Application.DTOs;

/// <summary>
/// Request để khởi tạo login flow
/// </summary>
public class LoginRequest
{
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Response trả về authorization URL
/// </summary>
public class LoginResponse
{
    public string AuthorizationUrl { get; set; } = string.Empty;
}

/// <summary>
/// Request cho callback sau khi login
/// </summary>
public class SignInCallbackRequest
{
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// Response sau khi login thành công
/// </summary>
public class SignInCallbackResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

/// <summary>
/// Request cho logout
/// </summary>
public class LogoutRequest
{
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// Response cho user info
/// </summary>
public class UserInfoResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Claims { get; set; } = new();
}

/// <summary>
/// Response cho session validation
/// </summary>
public class SessionValidationResponse
{
    public bool IsValid { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Session ID mới nếu đã bị rotate
    /// </summary>
    public string? NewSessionId { get; set; }
}
