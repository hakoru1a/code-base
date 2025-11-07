using ApiGateway.Services;
using ApiGateway.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiGateway.Middlewares;

/// <summary>
/// Middleware để validate session cho mỗi request
/// - Kiểm tra session cookie có tồn tại không
/// - Validate session trong Redis
/// - Refresh token nếu cần
/// - Parse JWT và set HttpContext.User (for RBAC)
/// - Set user context cho downstream
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;
    private readonly JwtSecurityTokenHandler _jwtHandler;

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
        _jwtHandler = new JwtSecurityTokenHandler();
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

        // 2. Validate và lấy session
        var session = await ValidateAndGetSessionAsync(context, sessionManager, path);
        if (session is null)
        {
            return; // Response đã được set trong ValidateAndGetSessionAsync
        }

        // 3. Refresh token nếu cần
        var refreshResult = await RefreshTokenIfNeededAsync(
            context, sessionManager, oauthClient, session);
        if (!refreshResult)
        {
            return; // Response đã được set trong RefreshTokenIfNeededAsync
        }

        // 4. Parse JWT và set HttpContext.User để RBAC hoạt động
        SetUserContextFromJwt(context, session);

        // 5. Set user info vào HttpContext.Items để downstream có thể access
        SetUserContextItems(context, session);

        // 6. Continue pipeline
        await _next(context);
    }

    /// <summary>
    /// Validate session cookie và lấy session từ Redis
    /// </summary>
    private async Task<UserSession?> ValidateAndGetSessionAsync(
        HttpContext context,
        ISessionManager sessionManager,
        string path)
    {
        // Lấy session ID từ cookie
        if (!context.Request.Cookies.TryGetValue("session_id", out var sessionId) ||
            string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("No session cookie found for path: {Path}", path);
            await WriteUnauthorizedResponseAsync(context, "unauthorized",
                "Session not found. Please login.");
            return null;
        }

        // Lấy session từ Redis
        var session = await sessionManager.GetSessionAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Session not found in Redis: {SessionId}", sessionId);
            await WriteUnauthorizedResponseAsync(context, "unauthorized",
                "Session expired. Please login again.");
            return null;
        }

        return session;
    }

    /// <summary>
    /// Refresh access token nếu cần thiết
    /// </summary>
    private async Task<bool> RefreshTokenIfNeededAsync(
        HttpContext context,
        ISessionManager sessionManager,
        IOAuthClient oauthClient,
        UserSession session)
    {
        if (!session.NeedsRefresh())
        {
            return true; // Không cần refresh
        }

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

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to refresh token for user: {Username}",
                session.Username);

            // Token refresh failed -> session invalid
            await sessionManager.RemoveSessionAsync(session.SessionId);

            await WriteUnauthorizedResponseAsync(context, "token_refresh_failed",
                "Failed to refresh token. Please login again.");

            return false;
        }
    }

    /// <summary>
    /// Parse JWT và set HttpContext.User để RBAC hoạt động
    /// </summary>
    private void SetUserContextFromJwt(HttpContext context, UserSession session)
    {
        try
        {
            var jwtToken = _jwtHandler.ReadJwtToken(session.AccessToken);
            var claims = jwtToken.Claims.ToList();

            // Tạo ClaimsIdentity với authentication type
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Set user context cho ASP.NET Core Authorization
            context.User = principal;

            _logger.LogDebug(
                "JWT parsed successfully for user: {Username}, Roles: {Roles}",
                session.Username,
                string.Join(", ", claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to parse JWT for user: {Username}. RBAC may not work properly.",
                session.Username);
        }
    }

    /// <summary>
    /// Set user info vào HttpContext.Items để downstream có thể access
    /// </summary>
    private static void SetUserContextItems(HttpContext context, UserSession session)
    {
        context.Items["UserSession"] = session;
        context.Items["AccessToken"] = session.AccessToken;
        context.Items["UserId"] = session.UserId;
        context.Items["Username"] = session.Username;
    }

    /// <summary>
    /// Write unauthorized response
    /// </summary>
    private static async Task WriteUnauthorizedResponseAsync(
        HttpContext context,
        string error,
        string message)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new
        {
            error,
            message
        });
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

