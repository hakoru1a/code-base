using Microsoft.OpenApi.Models;

namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for Swagger configuration
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configure Swagger documentation for Auth API
    /// </summary>
    public static IServiceCollection AddAuthSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Auth API",
                Version = "v1",
                Description = "Authentication service xử lý OAuth 2.0/OIDC với Keycloak"
            });
        });

        return services;
    }

    /// <summary>
    /// Use Swagger UI for development environment
    /// </summary>
    public static IApplicationBuilder UseAuthSwagger(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}

