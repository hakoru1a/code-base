namespace Shared.DTOs.Auth;

/// <summary>
/// Request cho logout
/// </summary>
public class LogoutRequest
{
    public string SessionId { get; set; } = string.Empty;
}

