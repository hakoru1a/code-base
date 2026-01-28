using ApiGateway.Configurations;
using Microsoft.OpenApi.Models;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for Swagger configuration
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configure Swagger documentation
    /// </summary>
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Gateway",
                Version = "v1",
                Description = @"API Gateway"
            });

            // Add JWT Bearer security definition for JWT-only approach
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger UI with downstream services
    /// </summary>
    public static IApplicationBuilder UseGatewaySwaggerUI(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        ServicesOptions servicesOptions)
    {
        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            // API Gateway endpoint
            c.SwaggerEndpoint(SwaggerOptions.ApiGatewayEndpoint, SwaggerOptions.ApiGatewayTitle);

            // Add TLBIOMASS API if configured
            if (servicesOptions?.TLBIOMASS != null && servicesOptions.TLBIOMASS.IncludeInSwagger && !string.IsNullOrEmpty(servicesOptions.TLBIOMASS.Url))
            {
                var swaggerUrl = $"{servicesOptions.TLBIOMASS.Url}/swagger/v1/swagger.json";
                c.SwaggerEndpoint(swaggerUrl, servicesOptions.TLBIOMASS.Name);
            }

            c.RoutePrefix = SwaggerOptions.RoutePrefix;

            if (environment.IsDevelopment())
            {
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.EnableValidator();
            }
        });

        return app;
    }
}








