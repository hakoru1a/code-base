using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiGateway.Services;
using ApiGateway.Configurations;
using Infrastructure.Identity;

namespace ApiGateway.Middlewares;

/// <summary>
/// Middleware để validate session cho mỗi request
/// Validate session trực tiếp và refresh token nếu cần
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(
        RequestDelegate next,
        ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISessionManager sessionManager,
        IOAuthClient oauthClient,
        IClientFingerprintService fingerprintService,
        IJwtClaimsCache jwtClaimsCache)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. Skip validation cho public paths
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 2. Get session ID from cookie
        if (!context.Request.Cookies.TryGetValue(CookieConstants.SessionIdCookieName, out var sessionId) ||
            string.IsNullOrEmpty(sessionId))
        {
            await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                "Session not found or expired. Please login.");
            return;
        }

        // 3. Get session from Redis
        var session = await sessionManager.GetSessionAsync(sessionId);

        if (session == null)
        {
            // Session không tồn tại hoặc đã bị invalidated
            // Xóa cookie để tránh client tiếp tục gửi session_id cũ trong các request tiếp theo
            DeleteSessionCookie(context);
            await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                "Session not found or expired. Please login.");
            return;
        }

        // 4. Validate session context (fingerprint, etc.)
        if (!await sessionManager.ValidateSessionContextAsync(sessionId, context))
        {
            // Session validation failed (có thể do fingerprint mismatch)
            // ValidateSessionContextAsync đã gọi InvalidateSessionAsync, session đã bị đánh dấu invalid
            // Xóa cookie để client không tiếp tục gửi session_id đã invalidated
            DeleteSessionCookie(context);
            await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                "Session validation failed. Please login again.");
            return;
        }

        string? newSessionId = null;

        // 5. Check if session needs rotation (> 10 phút kể từ lần rotate cuối hoặc từ lúc tạo)
        var lastRotateTime = session.LastRotatedAt ?? session.CreatedAt;
        var minutesSinceLastRotate = (DateTime.UtcNow - lastRotateTime).TotalMinutes;

        if (minutesSinceLastRotate >= 10)
        {
            try
            {
                newSessionId = await sessionManager.RotateSessionIdAsync(session.SessionId);

                // Lấy lại session mới sau khi rotate
                var rotatedSession = await sessionManager.GetSessionAsync(newSessionId);

                if (rotatedSession == null)
                {
                    _logger.LogError("Failed to get session after rotation: {NewSessionId}", newSessionId);
                    await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                        "Session error. Please login again.");
                    return;
                }

                session = rotatedSession;

                _logger.LogInformation(
                    "Session rotated: {OldSessionId} -> {NewSessionId}, Minutes since last rotate: {Minutes}",
                    sessionId,
                    newSessionId,
                    minutesSinceLastRotate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate session: {SessionId}", sessionId);
                // Continue với session cũ nếu rotate fail
            }
        }

        // 6. Update cookie nếu session đã được rotate
        if (!string.IsNullOrEmpty(newSessionId))
        {
            context.Response.Cookies.Append(
                CookieConstants.SessionIdCookieName,
                newSessionId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    MaxAge = TimeSpan.FromDays(7)
                });

            _logger.LogInformation(
                "Session cookie updated with new session ID: {NewSessionId}",
                newSessionId);
        }

        // 7. Check if token needs refresh using cached expiration check
        var needsRefresh = await jwtClaimsCache.IsTokenNearExpirationAsync(session.AccessToken);
        if (needsRefresh)
        {
            try
            {
                var tokenResponse = await oauthClient.RefreshTokenAsync(session.RefreshToken);

                session.AccessToken = tokenResponse.AccessToken;
                session.RefreshToken = tokenResponse.RefreshToken ?? session.RefreshToken;
                session.ExpiresAt = tokenResponse.CalculateExpiresAt();

                await sessionManager.UpdateSessionAsync(session);

                _logger.LogInformation("Token refreshed for session: {SessionId}", session.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh token for session: {SessionId}", session.SessionId);
                // Token refresh failed - xóa cookie để client login lại
                DeleteSessionCookie(context);
                await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                    "Token refresh failed. Please login again.");
                return;
            }
        }

        // 8. Parse JWT và set HttpContext.User để RBAC hoạt động (using cache)
        await SetUserContextFromJwtAsync(context, session.AccessToken, jwtClaimsCache);

        // 9. Set access token vào HttpContext.Items để TokenDelegatingHandler sử dụng
        context.Items[HttpContextItemKeys.AccessToken] = session.AccessToken;

        // 10. Continue pipeline
        await _next(context);
    }

    /// <summary>
    /// Parse JWT và set HttpContext.User để RBAC hoạt động (using cache for better performance)
    /// </summary>
    private async Task SetUserContextFromJwtAsync(HttpContext context, string? accessToken, IJwtClaimsCache jwtClaimsCache)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("AccessToken is null or empty. Cannot set claims.");
            return;
        }

        try
        {
            // Use cached claims principal for better performance
            var principal = await jwtClaimsCache.GetOrCreateClaimsAsync(accessToken);
            context.User = principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JWT from cache. RBAC may not work properly.");
        }
    }

    private static async Task WriteUnauthorizedResponseAsync(
        HttpContext context,
        string error,
        string message)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error, message });
    }

    private static bool IsPublicPath(string path)
    {
        if (PublicPaths.Paths.Contains(path))
            return true;

        foreach (var publicPath in PublicPaths.Paths)
        {
            if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Xóa session cookie khi session không hợp lệ hoặc đã bị invalidated
    /// Điều này ngăn client tiếp tục gửi session_id cũ trong các request tiếp theo
    /// </summary>
    private static void DeleteSessionCookie(HttpContext context)
    {
        try
        {
            context.Response.Cookies.Delete(CookieConstants.SessionIdCookieName, new CookieOptions
            {
                Path = CookieConstants.CookiePath,
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }
        catch
        {
            // Ignore errors when deleting cookie
        }
    }
}

public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}

