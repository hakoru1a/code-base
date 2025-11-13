using System.Text.Json;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for Health Check configuration
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Map health check endpoint with custom response writer
    /// </summary>
    public static IEndpointRouteBuilder MapGatewayHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.ToString(),
                        tags = e.Value.Tags
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });

        return endpoints;
    }
}





