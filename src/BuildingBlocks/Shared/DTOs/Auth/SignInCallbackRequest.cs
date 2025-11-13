namespace Shared.DTOs.Auth;

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

