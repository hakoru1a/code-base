namespace Shared.DTOs.Auth;

/// <summary>
/// Response trả về authorization URL
/// </summary>
public class LoginResponse
{
    public string AuthorizationUrl { get; set; } = string.Empty;
}

