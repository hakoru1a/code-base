namespace Shared.DTOs.Auth;

/// <summary>
/// Request để khởi tạo login flow
/// </summary>
public class LoginRequest
{
    public string? ReturnUrl { get; set; }
}

