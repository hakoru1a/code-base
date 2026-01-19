using ApiGateway.Models;

namespace ApiGateway.Services;

/// <summary>
/// Service để cache user claims từ JWT ID token
/// Thay thế Session Management trong JWT-only approach
/// </summary>
public interface IUserClaimsCache
{
    /// <summary>
    /// Cache user claims được extract từ JWT ID token
    /// </summary>
    /// <param name="idToken">JWT ID token từ Keycloak</param>
    /// <param name="expirationMinutes">Thời gian cache tồn tại (mặc định 60 phút)</param>
    Task CacheUserClaimsAsync(string idToken, int expirationMinutes = 60);

    /// <summary>
    /// Lấy cached user claims theo user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    Task<CachedUserClaims?> GetUserClaimsAsync(string userId);

    /// <summary>
    /// Remove cached user claims (dùng khi logout)
    /// </summary>
    /// <param name="userId">User ID</param>
    Task RemoveUserClaimsAsync(string userId);

    /// <summary>
    /// Kiểm tra user claims có tồn tại và còn hiệu lực không
    /// </summary>
    /// <param name="userId">User ID</param>
    Task<bool> IsUserClaimsValidAsync(string userId);
}