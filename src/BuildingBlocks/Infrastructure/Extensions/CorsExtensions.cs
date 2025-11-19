using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, string policyName = "AllowAllOrigins", Action<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>? configureOptions = null)
        {
            services.AddCors(options =>
            {
                if (configureOptions != null)
                {
                    configureOptions(options);
                }
                else
                {
                    // Default policy - Allow all origins (for development)
                    options.AddPolicy(policyName, builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
                }
            });

            return services;
        }

        public static IServiceCollection AddCorsForProduction(this IServiceCollection services, string[] allowedOrigins, string policyName = "ProductionCorsPolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddCorsForDevelopment(this IServiceCollection services, string policyName = "DevelopmentCorsPolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.SetIsOriginAllowed(_ => true) // Allow any origin
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app, string policyName = "AllowAllOrigins")
        {
            app.UseCors(policyName);
            return app;
        }
    }
}