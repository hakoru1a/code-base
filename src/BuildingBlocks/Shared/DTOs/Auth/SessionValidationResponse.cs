namespace Shared.DTOs.Auth;

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

