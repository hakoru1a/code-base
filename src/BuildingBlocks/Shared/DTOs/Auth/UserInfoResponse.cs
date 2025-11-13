namespace Shared.DTOs.Auth;

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

