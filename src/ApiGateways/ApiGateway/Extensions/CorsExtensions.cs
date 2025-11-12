using ApiGateway.Configurations;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for CORS configuration
/// </summary>
public static class CorsExtensions
{
    private const string PolicyName = "AllowWebApp";

    /// <summary>
    /// Add CORS policy for web application
    /// </summary>
    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        OAuthOptions oAuthOptions)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, corsBuilder =>
            {
                corsBuilder
                    .WithOrigins(oAuthOptions.WebAppUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Use CORS policy for web application
    /// </summary>
    public static IApplicationBuilder UseGatewayCors(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}


