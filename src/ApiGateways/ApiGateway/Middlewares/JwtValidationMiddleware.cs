using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiGateway.Configurations;
using Shared.SeedWork;

namespace ApiGateway.Middlewares;

/// <summary>
/// JWT Token Validation Middleware - Thay thế SessionValidationMiddleware
/// Validate Bearer token trong Authorization header cho JWT-only approach
/// </summary>
public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(
        RequestDelegate next,
        ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. Skip validation cho public paths
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 2. Skip validation cho endpoints có [AllowAnonymous]
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        // 3. Check Authorization header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Missing or invalid Authorization header for path: {Path}", path);
            await WriteUnauthorizedResponseAsync(context, "Authorization header is required");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Empty bearer token for path: {Path}", path);
            await WriteUnauthorizedResponseAsync(context, "Valid bearer token is required");
            return;
        }

        try
        {
            // 4. Basic JWT validation (signature validation handled by JWT middleware)
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                _logger.LogWarning("Invalid JWT token format for path: {Path}", path);
                await WriteUnauthorizedResponseAsync(context, "Invalid token format");
                return;
            }

            var jsonToken = handler.ReadJwtToken(token);

            // 5. Check token expiration
            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired JWT token for path: {Path}", path);
                await WriteUnauthorizedResponseAsync(context, "Token has expired");
                return;
            }

            // 6. Create claims principal từ JWT
            var claims = jsonToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            context.User = new ClaimsPrincipal(identity);

            // 7. Log successful validation
            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var username = jsonToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            _logger.LogDebug("JWT token validated successfully for user: {Username} ({UserId})", username, userId);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT token validation failed for path: {Path}", path);
            await WriteUnauthorizedResponseAsync(context, "Token validation failed");
        }
    }

    private static async Task WriteUnauthorizedResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var errorResponse = new ApiErrorResult<object>(message);
        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private static bool IsPublicPath(string path)
    {
        var publicPaths = new[]
        {
            "/auth/login",
            "/auth/signin-oidc",
            "/auth/health",
            "/health",
            "/swagger",
            "/_whoami",
            "/auth/exchange"
        };

        // Exact match
        if (publicPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
            return true;

        // Prefix match
        foreach (var publicPath in publicPaths)
        {
            if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

public static class JwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtValidationMiddleware>();
    }
}