using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ApiGateway.Configurations;

namespace ApiGateway.Middlewares;

/// <summary>
/// Middleware để validate session cho mỗi request
/// Gọi Auth service để validate session và lấy access token
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;
    private readonly JwtSecurityTokenHandler _jwtHandler;

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
        IHttpClientFactory httpClientFactory,
        IOptions<ServicesOptions> servicesOptions)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. Skip validation cho public paths
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 2. Validate session thông qua Auth service
        var sessionValidation = await ValidateSessionAsync(
            context, httpClientFactory, servicesOptions.Value);

        if (sessionValidation == null || !sessionValidation.IsValid)
        {
            await WriteUnauthorizedResponseAsync(context, AuthenticationConstants.Unauthorized,
                "Session not found or expired. Please login.");
            return;
        }

        // 3. Nếu session đã được rotate, update cookie với session_id mới
        if (!string.IsNullOrEmpty(sessionValidation.NewSessionId))
        {
            context.Response.Cookies.Append(
                CookieConstants.SessionIdCookieName,
                sessionValidation.NewSessionId,
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
                sessionValidation.NewSessionId);
        }

        // 4. Parse JWT và set HttpContext.User để RBAC hoạt động
        SetUserContextFromJwt(context, sessionValidation.AccessToken);

        // 5. Set access token vào HttpContext.Items để TokenDelegatingHandler sử dụng
        context.Items[HttpContextItemKeys.AccessToken] = sessionValidation.AccessToken;

        // 6. Continue pipeline
        await _next(context);
    }

    /// <summary>
    /// Validate session thông qua Auth service
    /// </summary>
    private async Task<SessionValidationResponse?> ValidateSessionAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        ServicesOptions servicesOptions)
    {
        try
        {
            if (!context.Request.Cookies.TryGetValue(CookieConstants.SessionIdCookieName, out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                _logger.LogWarning("No session cookie found");
                return null;
            }

            var client = httpClientFactory.CreateClient("AuthService");

            var response = await client.GetAsync($"/api/auth/session/{sessionId}/validate");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Session validation failed for session: {SessionId}", sessionId);
                return null;
            }

            var validation = await response.Content.ReadFromJsonAsync<SessionValidationResponse>();

            return validation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            // Re-throw the exception to allow upstream error handling
            // This ensures the root cause is not hidden
            throw new InvalidOperationException("Failed to validate session", ex);
        }
    }

    /// <summary>
    /// Parse JWT và set HttpContext.User để RBAC hoạt động
    /// </summary>
    private void SetUserContextFromJwt(HttpContext context, string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("AccessToken is null or empty. Cannot set claims.");
            return;
        }

        try
        {
            var jwtToken = _jwtHandler.ReadJwtToken(accessToken);
            var claims = jwtToken.Claims.ToList();

            var identity = new ClaimsIdentity(claims, AuthenticationConstants.BearerScheme);
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JWT. RBAC may not work properly.");
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

    #region DTOs

    private class SessionValidationResponse
    {
        public bool IsValid { get; set; }
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? NewSessionId { get; set; }
    }

    #endregion
}

public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}

