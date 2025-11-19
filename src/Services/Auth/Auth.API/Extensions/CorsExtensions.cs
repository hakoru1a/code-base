namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for CORS configuration
/// </summary>
public static class CorsExtensions
{
    private const string PolicyName = "AllowAll";

    /// <summary>
    /// Add CORS policy that allows all origins (for development)
    /// </summary>
    public static IServiceCollection AddAuthCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, corsBuilder =>
            {
                corsBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Use CORS policy
    /// </summary>
    public static IApplicationBuilder UseAuthCors(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}






