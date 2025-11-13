namespace Shared.DTOs.Auth;

/// <summary>
/// Response sau khi login thành công
/// </summary>
public class SignInCallbackResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

