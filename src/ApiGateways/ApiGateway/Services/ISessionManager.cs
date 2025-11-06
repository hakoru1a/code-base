using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// Interface cho Session Manager
/// Quản lý user sessions trong Redis
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Tạo session mới và lưu vào Redis
    /// </summary>
    /// <param name="tokenResponse">Token response từ Keycloak</param>
    /// <returns>Session ID (để lưu vào cookie)</returns>
    Task<string> CreateSessionAsync(TokenResponse tokenResponse);

    /// <summary>
    /// Lấy session từ Redis theo session ID
    /// </summary>
    /// <param name="sessionId">Session ID từ cookie</param>
    /// <returns>User session nếu tồn tại và valid, null nếu không</returns>
    Task<UserSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Update session trong Redis (dùng khi refresh token)
    /// </summary>
    /// <param name="session">Session cần update</param>
    Task UpdateSessionAsync(UserSession session);

    /// <summary>
    /// Xóa session khỏi Redis (logout)
    /// </summary>
    /// <param name="sessionId">Session ID cần xóa</param>
    Task RemoveSessionAsync(string sessionId);

    /// <summary>
    /// Update last accessed time (cho sliding expiration)
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    Task UpdateLastAccessedAsync(string sessionId);

    /// <summary>
    /// Kiểm tra session có hợp lệ không
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>True nếu session tồn tại và chưa expire</returns>
    Task<bool> IsValidSessionAsync(string sessionId);
}

