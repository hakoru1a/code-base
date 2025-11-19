using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
        {
            services.AddHealthChecks();
            return services;
        }

        public static IServiceCollection AddHealthCheckWithDatabase(this IServiceCollection services, string connectionString, string name = "database")
        {
            services.AddHealthChecks()
                    .AddSqlServer(connectionString, name: name);
            return services;
        }

        public static IServiceCollection AddHealthCheckWithRedis(this IServiceCollection services, string connectionString, string name = "redis")
        {
            services.AddHealthChecks()
                    .AddRedis(connectionString, name: name);
            return services;
        }

        public static IServiceCollection AddHealthCheckWithRabbitMQ(this IServiceCollection services, string connectionString, string name = "rabbitmq")
        {
            services.AddHealthChecks()
                    .AddRabbitMQ(connectionString, name: name);
            return services;
        }

        public static IServiceCollection AddHealthCheckWithHttpClient(this IServiceCollection services, Uri uri, string name = "http_client")
        {
            services.AddHealthChecks()
                    .AddCheck(name, () =>
                    {
                        try
                        {
                            using var client = new HttpClient();
                            var response = client.GetAsync(uri).Result;
                            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                        }
                        catch
                        {
                            return HealthCheckResult.Unhealthy();
                        }
                    });
            return services;
        }

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, 
            Func<IServiceProvider, IHealthCheck> factory, 
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
        {
            var healthCheckBuilder = services.AddHealthChecks();
            
            healthCheckBuilder.Add(new HealthCheckRegistration(
                name,
                factory,
                failureStatus,
                tags,
                timeout));

            return services;
        }

        public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app, string endpoint = "/health")
        {
            app.UseHealthChecks(endpoint, new HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            return app;
        }

        public static IApplicationBuilder UseDetailedHealthCheck(this IApplicationBuilder app, string endpoint = "/health/detailed")
        {
            app.UseHealthChecks(endpoint, new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });

            return app;
        }

        public static IApplicationBuilder UseHealthCheckWithUI(this IApplicationBuilder app, 
            string healthEndpoint = "/health", 
            string detailedEndpoint = "/health/detailed",
            string uiEndpoint = "/health-ui")
        {
            // Basic health check
            app.UseHealthChecks(healthEndpoint, new HealthCheckOptions
            {
                Predicate = _ => false // Exclude all checks for basic endpoint
            });

            // Detailed health check
            app.UseHealthChecks(detailedEndpoint, new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse
            });

            return app;
        }

        private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                status = result.Status.ToString(),
                timestamp = DateTime.UtcNow,
                duration = result.TotalDuration
            };

            var jsonString = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonString);
        }

        private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                status = result.Status.ToString(),
                timestamp = DateTime.UtcNow,
                duration = result.TotalDuration,
                checks = result.Entries.Select(pair => new
                {
                    name = pair.Key,
                    status = pair.Value.Status.ToString(),
                    description = pair.Value.Description,
                    duration = pair.Value.Duration,
                    exception = pair.Value.Exception?.Message,
                    data = pair.Value.Data
                })
            };

            var jsonString = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonString);
        }
    }
}