using Auth.Domain.Models;

namespace Auth.Application.Interfaces;

/// <summary>
/// Session Manager interface
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Tạo session mới từ token response
    /// </summary>
    Task<string> CreateSessionAsync(TokenResponse tokenResponse);

    /// <summary>
    /// Lấy session từ Redis
    /// </summary>
    Task<UserSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Update session (sau khi refresh token)
    /// </summary>
    Task UpdateSessionAsync(UserSession session);

    /// <summary>
    /// Xóa session (logout)
    /// </summary>
    Task RemoveSessionAsync(string sessionId);

    /// <summary>
    /// Update last accessed time
    /// </summary>
    Task UpdateLastAccessedAsync(string sessionId);

    /// <summary>
    /// Kiểm tra session validity
    /// </summary>
    Task<bool> IsValidSessionAsync(string sessionId);

    /// <summary>
    /// Rotate session_id (tạo session_id mới, đưa session_id cũ vào will_remove list)
    /// </summary>
    /// <param name="oldSessionId">Session ID cũ</param>
    /// <returns>Session ID mới</returns>
    Task<string> RotateSessionIdAsync(string oldSessionId);
}
