using ApiGateway.Services;

namespace ApiGateway.Middlewares;

/// <summary>
/// Middleware để validate session cho mỗi request
/// - Kiểm tra session cookie có tồn tại không
/// - Validate session trong Redis
/// - Refresh token nếu cần
/// - Set user context cho downstream
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    // Paths không cần authentication
    private static readonly HashSet<string> PublicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/auth/login",
        "/auth/signin-oidc",
        "/auth/logout",
        "/auth/signout-callback-oidc"
    };

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
        IOAuthClient oauthClient)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. Skip validation cho public paths
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 2. Lấy session ID từ cookie
        if (!context.Request.Cookies.TryGetValue("session_id", out var sessionId) ||
            string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("No session cookie found for path: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "unauthorized",
                message = "Session not found. Please login."
            });
            return;
        }

        // 3. Lấy session từ Redis
        var session = await sessionManager.GetSessionAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Session not found in Redis: {SessionId}", sessionId);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "unauthorized",
                message = "Session expired. Please login again."
            });
            return;
        }

        // 4. Kiểm tra token expiration và ssh nếu cần
        if (session.NeedsRefresh())
        {
            _logger.LogInformation(
                "Access token needs refresh for user: {Username}",
                session.Username);

            try
            {
                // Refresh token
                var tokenResponse = await oauthClient.RefreshTokenAsync(session.RefreshToken);

                // Update session với token mới
                session.AccessToken = tokenResponse.AccessToken;
                session.RefreshToken = tokenResponse.RefreshToken ?? session.RefreshToken;
                session.ExpiresAt = tokenResponse.CalculateExpiresAt();

                await sessionManager.UpdateSessionAsync(session);

                _logger.LogInformation(
                    "Token refreshed successfully for user: {Username}",
                    session.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to refresh token for user: {Username}",
                    session.Username);

                // Token refresh failed -> session invalid
                await sessionManager.RemoveSessionAsync(sessionId);

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "token_refresh_failed",
                    message = "Failed to refresh token. Please login again."
                });
                return;
            }
        }

        // 5. Set user info vào HttpContext.Items để downstream có thể access
        context.Items["UserSession"] = session;
        context.Items["AccessToken"] = session.AccessToken;
        context.Items["UserId"] = session.UserId;
        context.Items["Username"] = session.Username;

        // 6. Continue pipeline
        await _next(context);
    }

    /// <summary>
    /// Kiểm tra path có phải public path không
    /// </summary>
    private static bool IsPublicPath(string path)
    {
        // Exact match
        if (PublicPaths.Contains(path))
            return true;

        // Starts with match (for swagger paths)
        foreach (var publicPath in PublicPaths)
        {
            if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Extension method để register middleware
/// </summary>
public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}

