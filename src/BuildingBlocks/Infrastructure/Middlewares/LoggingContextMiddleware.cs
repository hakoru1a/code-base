using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Security.Claims;

namespace Infrastructure.Middlewares
{
    /// <summary>
    /// Middleware để thêm correlation ID và username vào logging context
    /// Tự động tạo correlation ID cho mỗi request và track username từ JWT claims
    /// </summary>
    public class LoggingContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingContextMiddleware> _logger;

        public LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Lấy hoặc tạo correlation ID
            var correlationId = GetOrCreateCorrelationId(context);
            
            // 2. Lấy username từ JWT claims
            var username = GetUsernameFromContext(context);
            
            // 3. Lấy thông tin request cơ bản
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress(context);
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;

            // 4. Push properties vào Serilog LogContext
            using var correlationIdScope = LogContext.PushProperty("CorrelationId", correlationId);
            using var usernameScope = LogContext.PushProperty("Username", username ?? "anonymous");
            using var userAgentScope = LogContext.PushProperty("UserAgent", userAgent);
            using var ipAddressScope = LogContext.PushProperty("ClientIP", ipAddress);
            using var requestPathScope = LogContext.PushProperty("RequestPath", requestPath);
            using var requestMethodScope = LogContext.PushProperty("RequestMethod", requestMethod);

            // 5. Thêm correlation ID vào response header để client có thể track
            context.Response.Headers.TryAdd("X-Correlation-Id", correlationId);

            // 6. Log bắt đầu request
            _logger.LogInformation(
                "Request started - {RequestMethod} {RequestPath} | User: {Username} | CorrelationId: {CorrelationId} | IP: {ClientIP}",
                requestMethod, requestPath, username ?? "anonymous", correlationId, ipAddress);

            try
            {
                // 7. Tiếp tục middleware pipeline
                await _next(context);

                // 8. Log kết thúc request thành công
                _logger.LogInformation(
                    "Request completed - {RequestMethod} {RequestPath} | StatusCode: {StatusCode} | User: {Username} | CorrelationId: {CorrelationId}",
                    requestMethod, requestPath, context.Response.StatusCode, username ?? "anonymous", correlationId);
            }
            catch (Exception ex)
            {
                // 9. Log lỗi với correlation ID và username
                _logger.LogError(ex,
                    "Request failed - {RequestMethod} {RequestPath} | User: {Username} | CorrelationId: {CorrelationId} | Error: {ErrorMessage}",
                    requestMethod, requestPath, username ?? "anonymous", correlationId, ex.Message);

                // Re-throw để middleware error handling khác xử lý
                throw;
            }
        }

        /// <summary>
        /// Lấy correlation ID từ header hoặc tạo mới nếu chưa có
        /// </summary>
        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Kiểm tra header X-Correlation-Id từ client (có thể từ API Gateway hoặc frontend)
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerCorrelationId) &&
                !string.IsNullOrWhiteSpace(headerCorrelationId))
            {
                return headerCorrelationId.ToString();
            }

            // Kiểm tra HttpContext.TraceIdentifier (built-in ASP.NET Core)
            if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
            {
                return context.TraceIdentifier;
            }

            // Tạo correlation ID mới
            var newCorrelationId = Guid.NewGuid().ToString("N")[..12]; // Lấy 12 ký tự đầu để ngắn gọn
            
            // Lưu vào HttpContext.Items để middleware khác có thể sử dụng
            context.Items["CorrelationId"] = newCorrelationId;
            
            return newCorrelationId;
        }

        /// <summary>
        /// Lấy username từ JWT claims trong HttpContext.User
        /// </summary>
        private string? GetUsernameFromContext(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Thử các claim types phổ biến cho username
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value ??
                              context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              context.User.FindFirst("preferred_username")?.Value ??
                              context.User.FindFirst("username")?.Value ??
                              context.User.FindFirst("name")?.Value;

                if (!string.IsNullOrWhiteSpace(username))
                {
                    return username;
                }

                // Nếu không tìm thấy username, lấy subject ID
                return context.User.FindFirst("sub")?.Value;
            }

            return null;
        }

        /// <summary>
        /// Lấy IP address của client, xử lý trường hợp có proxy/load balancer
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            // Kiểm tra các header proxy phổ biến
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // X-Forwarded-For có thể chứa nhiều IP, lấy cái đầu tiên
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp;
            }

            // Fallback về connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}