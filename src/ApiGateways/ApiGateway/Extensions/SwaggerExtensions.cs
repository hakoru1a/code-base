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
                Description = @"
                    API Gateway - Simple Routing & Session Management
                    
                    Architecture:
                    - Gateway chỉ đảm nhận routing và session validation đơn giản
                    - Toàn bộ OAuth 2.0/OIDC logic được xử lý bởi Auth Service
                    - RBAC/PBAC được thực thi tại Gateway và Backend Services
                    
                    Authentication Flow:
                    1. GET /auth/login → Proxy tới Auth Service
                    2. Auth Service xử lý OAuth 2.0 + PKCE với Keycloak
                    3. GET /auth/signin-oidc → Proxy tới Auth Service
                    4. Auth Service tạo session và lưu vào Redis
                    5. Gateway nhận session_id và set HttpOnly cookie
                    6. Các API calls được validate session qua Auth Service
                    7. Gateway inject Bearer token vào requests tới downstream services
                    
                    Security Features:
                    - Session-based authentication với HttpOnly cookies
                    - Token management tại Auth Service (không ở Gateway)
                    - Session validation middleware
                    - Automatic Bearer token injection
                    - RBAC/PBAC policy enforcement
                "
            });

            // Add security definition (informational only)
            c.AddSecurityDefinition("Session", new OpenApiSecurityScheme
            {
                Description = "Session-based authentication using HttpOnly cookies",
                Name = CookieConstants.SessionIdCookieName,
                In = ParameterLocation.Cookie,
                Type = SecuritySchemeType.ApiKey
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

        if (environment.IsDevelopment())
        {
            app.UseSwaggerUI(c =>
            {
                // API Gateway endpoint
                c.SwaggerEndpoint(SwaggerOptions.ApiGatewayEndpoint, SwaggerOptions.ApiGatewayTitle);
                
                // Downstream services endpoints (only if enabled)
                if (servicesOptions.BaseAPI.IncludeInSwagger)
                {
                    c.SwaggerEndpoint(
                        $"{servicesOptions.BaseAPI.Url}/swagger/v1/swagger.json",
                        servicesOptions.BaseAPI.Name);
                }
                
                if (servicesOptions.GenerateAPI.IncludeInSwagger)
                {
                    c.SwaggerEndpoint(
                        $"{servicesOptions.GenerateAPI.Url}/swagger/v1/swagger.json",
                        servicesOptions.GenerateAPI.Name);
                }

                if (servicesOptions.AuthAPI.IncludeInSwagger)
                {
                    c.SwaggerEndpoint(
                        $"{servicesOptions.AuthAPI.Url}/swagger/v1/swagger.json",
                        servicesOptions.AuthAPI.Name);
                }
                
                c.RoutePrefix = SwaggerOptions.RoutePrefix;
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.EnableValidator();
            });
        }
        else
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(SwaggerOptions.ApiGatewayEndpoint, SwaggerOptions.ApiGatewayTitle);
                c.RoutePrefix = SwaggerOptions.RoutePrefix;
            });
        }

        return app;
    }
}



