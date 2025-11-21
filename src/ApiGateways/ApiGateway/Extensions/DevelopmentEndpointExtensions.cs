using ApiGateway.Configurations;
using Microsoft.AspNetCore.Authentication;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for development-only endpoints
/// </summary>
public static class DevelopmentEndpointExtensions
{
    /// <summary>
    /// Map development endpoints like _whoami
    /// </summary>
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(
        this IEndpointRouteBuilder endpoints,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return endpoints;

        endpoints.MapGet("/_whoami", async (HttpContext ctx) =>
        {
            var user = ctx.User;

            // Lấy access token từ cookie auth
            var accessToken = await ctx.GetTokenAsync(TokenConstants.AccessToken);
            var refreshToken = await ctx.GetTokenAsync(TokenConstants.RefreshToken);
            var idToken = await ctx.GetTokenAsync(TokenConstants.IdToken);

            return Results.Json(new
            {
                user = new
                {
                    sub = user.FindFirst(ClaimConstants.Sub)?.Value,
                    username = user.Identity?.Name,
                    realm_roles = user.FindAll(ClaimConstants.RealmRoles).Select(c => c.Value),
                    resource_roles = user.FindAll(ClaimConstants.ResourceAccess).Select(c => c.Value),
                },
                tokens = new
                {
                    access_token = accessToken,
                    refresh_token = refreshToken, // Chỉ trả trong DEV
                    id_token = idToken
                },
                request = new
                {
                    traceId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
                    correlationId = ctx.Request.Headers[HttpHeaderConstants.CorrelationId].FirstOrDefault()
                }
            });
        })
        .RequireAuthorization();

        return endpoints;
    }
}











