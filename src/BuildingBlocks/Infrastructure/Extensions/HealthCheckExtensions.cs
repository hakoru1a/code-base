using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho Health Checks - Monitor sức khỏe của application và dependencies
    /// 
    /// MỤC ĐÍCH:
    /// - Expose health check endpoints để monitoring systems (Kubernetes, Azure, AWS) kiểm tra
    /// - Check connectivity tới dependencies: Database, Redis, RabbitMQ, External APIs
    /// - Graceful degradation: Phát hiện sớm khi services downstream có vấn đề
    /// - Hỗ trợ orchestration: Kubernetes dùng health checks để restart unhealthy pods
    /// 
    /// SỬ DỤNG:
    /// 1. Đăng ký health checks:
    ///    services.AddHealthCheckWithDatabase(connectionString)
    ///            .AddHealthCheckWithRedis(redisConnectionString)
    ///            .AddHealthCheckWithRabbitMQ(rabbitMqConnectionString);
    /// 
    /// 2. Expose health check endpoints:
    ///    app.UseHealthCheckConfiguration("/health");
    ///    app.UseDetailedHealthCheck("/health/detailed");
    /// 
    /// 3. Endpoints:
    ///    GET /health          → { "status": "Healthy" }
    ///    GET /health/detailed → { "status": "Healthy", "checks": [...], "duration": "50ms" }
    /// 
    /// KUBERNETES INTEGRATION:
    /// livenessProbe:
    ///   httpGet:
    ///     path: /health
    ///     port: 80
    /// readinessProbe:
    ///   httpGet:
    ///     path: /health
    ///     port: 80
    /// 
    /// IMPACT:
    /// + Monitoring: Ops team biết ngay khi service unhealthy
    /// + Auto-Recovery: Kubernetes tự động restart unhealthy pods
    /// + Troubleshooting: Detailed endpoint cho biết dependency nào đang lỗi
    /// + SLA: Giảm downtime nhờ phát hiện và xử lý sớm
    /// - Overhead: Mỗi health check gọi tới dependencies, tăng load
    /// - False Positives: Network hiccup có thể trigger unnecessary restarts
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Đăng ký health check cơ bản (không check dependencies)
        /// 
        /// CÁCH DÙNG: services.AddHealthCheckConfiguration();
        /// PHÙ HỢP: Application không có external dependencies
        /// </summary>
        public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
        {
            services.AddHealthChecks();
            return services;
        }

        /// <summary>
        /// Thêm health check cho SQL Server database
        /// 
        /// CÁCH DÙNG:
        /// services.AddHealthCheckWithDatabase(
        ///     configuration.GetConnectionString("DefaultConnection"),
        ///     name: "sql_server"
        /// );
        /// 
        /// KIỂM TRA: Thử connect và execute simple query tới database
        /// </summary>
        public static IServiceCollection AddHealthCheckWithDatabase(this IServiceCollection services, string connectionString, string name = "database")
        {
            services.AddHealthChecks()
                    .AddSqlServer(connectionString, name: name);
            return services;
        }

        /// <summary>
        /// Thêm health check cho Redis cache
        /// 
        /// CÁCH DÙNG:
        /// services.AddHealthCheckWithRedis("localhost:6379,password=secret", name: "redis_cache");
        /// 
        /// KIỂM TRA: Thử PING command tới Redis server
        /// </summary>
        public static IServiceCollection AddHealthCheckWithRedis(this IServiceCollection services, string connectionString, string name = "redis")
        {
            services.AddHealthChecks()
                    .AddRedis(connectionString, name: name);
            return services;
        }

        /// <summary>
        /// Thêm health check cho RabbitMQ message broker
        /// 
        /// CÁCH DÙNG:
        /// services.AddHealthCheckWithRabbitMQ("amqp://guest:guest@localhost:5672", name: "message_bus");
        /// 
        /// KIỂM TRA: Thử connect tới RabbitMQ và kiểm tra broker status
        /// </summary>
        public static IServiceCollection AddHealthCheckWithRabbitMQ(this IServiceCollection services, string connectionString, string name = "rabbitmq")
        {
            services.AddHealthChecks()
                    .AddRabbitMQ(connectionString, name: name);
            return services;
        }

        /// <summary>
        /// Thêm health check cho external HTTP APIs
        /// 
        /// CÁCH DÙNG:
        /// services.AddHealthCheckWithHttpClient(
        ///     new Uri("https://payment-api.com/health"),
        ///     name: "payment_service"
        /// );
        /// 
        /// KIỂM TRA: Gọi GET tới URI và check response status code (200 = Healthy)
        /// LƯU Ý: Synchronous call, có thể block thread
        /// </summary>
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

        /// <summary>
        /// Đăng ký custom health check logic
        /// 
        /// CÁCH DÙNG:
        /// services.AddCustomHealthCheck(
        ///     sp => new DiskSpaceHealthCheck("/data", minimumFreeMB: 1000),
        ///     name: "disk_space",
        ///     failureStatus: HealthStatus.Degraded,
        ///     tags: new[] { "storage" },
        ///     timeout: TimeSpan.FromSeconds(5)
        /// );
        /// 
        /// PHÙ HỢP: Business-specific health checks (disk space, license validation, custom metrics)
        /// </summary>
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

        /// <summary>
        /// Expose health check endpoint với simple JSON response
        /// 
        /// CÁCH DÙNG:
        /// app.UseHealthCheckConfiguration("/health");
        /// 
        /// RESPONSE:
        /// { "status": "Healthy", "timestamp": "2024-01-01T10:00:00Z", "duration": "50ms" }
        /// 
        /// PHÙ HỢP: Kubernetes liveness/readiness probes
        /// </summary>
        public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app, string endpoint = "/health")
        {
            app.UseHealthChecks(endpoint, new HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            return app;
        }

        /// <summary>
        /// Expose detailed health check endpoint với thông tin từng dependency
        /// 
        /// CÁCH DÙNG:
        /// app.UseDetailedHealthCheck("/health/detailed");
        /// 
        /// RESPONSE:
        /// {
        ///   "status": "Healthy",
        ///   "checks": [
        ///     { "name": "database", "status": "Healthy", "duration": "30ms" },
        ///     { "name": "redis", "status": "Healthy", "duration": "10ms" }
        ///   ]
        /// }
        /// 
        /// PHÙ HỢP: Troubleshooting, monitoring dashboards
        /// LƯU Ý: Không expose public vì chứa thông tin nhạy cảm về infrastructure
        /// </summary>
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