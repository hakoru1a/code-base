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

            // 2. Lấy thông tin request cơ bản
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress(context);
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;

            // 3. Lấy username từ context (có thể chưa có vì chưa qua authentication middleware)
            var initialUsername = GetUsernameFromContext(context);

            // 4. Push correlation ID và thông tin cơ bản vào Serilog LogContext
            using var correlationIdScope = LogContext.PushProperty("CorrelationId", correlationId);
            using var userAgentScope = LogContext.PushProperty("UserAgent", userAgent);
            using var ipAddressScope = LogContext.PushProperty("ClientIP", ipAddress);
            using var requestPathScope = LogContext.PushProperty("RequestPath", requestPath);
            using var requestMethodScope = LogContext.PushProperty("RequestMethod", requestMethod);

            // 5. Thêm correlation ID vào response header để client có thể track
            context.Response.Headers.TryAdd("X-Correlation-Id", correlationId);

            // 6. Log bắt đầu request (có thể chưa có username)
            _logger.LogInformation(
                "Request started - {RequestMethod} {RequestPath} | User: {Username} | CorrelationId: {CorrelationId} | IP: {ClientIP}",
                requestMethod, requestPath, initialUsername ?? "anonymous", correlationId, ipAddress);

            try
            {
                // 7. Tiếp tục middleware pipeline
                await _next(context);

                // 8. Lấy username lại sau khi pipeline hoàn thành (có thể đã có user context)
                var finalUsername = GetUsernameFromContext(context);
                
                // 9. Push username vào context cho các log sau này
                using var usernameScope = LogContext.PushProperty("Username", finalUsername ?? "anonymous");

                // 10. Log kết thúc request thành công với username đã cập nhật
                _logger.LogInformation(
                    "Request completed - {RequestMethod} {RequestPath} | StatusCode: {StatusCode} | User: {Username} | CorrelationId: {CorrelationId}",
                    requestMethod, requestPath, context.Response.StatusCode, finalUsername ?? "anonymous", correlationId);
            }
            catch (Exception ex)
            {
                // 11. Lấy username lại trong trường hợp có lỗi
                var errorUsername = GetUsernameFromContext(context);
                
                // 12. Log lỗi với correlation ID và username
                _logger.LogError(ex,
                    "Request failed - {RequestMethod} {RequestPath} | User: {Username} | CorrelationId: {CorrelationId} | Error: {ErrorMessage}",
                    requestMethod, requestPath, errorUsername ?? "anonymous", correlationId, ex.Message);

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
            // DEBUG: Log thông tin về HttpContext.User
            var userExists = context.User != null;
            var identityExists = context.User?.Identity != null;
            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

            _logger.LogInformation(
                "[DEBUG Username] User exists: {UserExists}, Identity exists: {IdentityExists}, IsAuthenticated: {IsAuthenticated}",
                userExists, identityExists, isAuthenticated);

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // DEBUG: Log tất cả claims có trong JWT
                var allClaims = context.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                _logger.LogInformation(
                    "[DEBUG Username] Found {ClaimCount} claims: {Claims}",
                    allClaims.Count,
                    string.Join(" | ", allClaims));

                // Thử các claim types phổ biến cho username
                var claimName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var claimNameIdentifier = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var claimPreferredUsername = context.User.FindFirst("preferred_username")?.Value;
                var claimUsername = context.User.FindFirst("username")?.Value;
                var claimNameDirect = context.User.FindFirst("name")?.Value;
                var claimSub = context.User.FindFirst("sub")?.Value;

                // DEBUG: Log từng claim type
                _logger.LogInformation(
                    "[DEBUG Username] Claim values - ClaimTypes.Name: {ClaimName}, " +
                    "ClaimTypes.NameIdentifier: {ClaimNameIdentifier}, " +
                    "preferred_username: {PreferredUsername}, " +
                    "username: {Username}, " +
                    "name: {Name}, " +
                    "sub: {Sub}",
                    claimName ?? "NULL",
                    claimNameIdentifier ?? "NULL",
                    claimPreferredUsername ?? "NULL",
                    claimUsername ?? "NULL",
                    claimNameDirect ?? "NULL",
                    claimSub ?? "NULL");

                var username = claimName ??
                              claimNameIdentifier ??
                              claimPreferredUsername ??
                              claimUsername ??
                              claimNameDirect;

                if (!string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogInformation("[DEBUG Username] Found username: {Username}", username);
                    return username;
                }

                // Nếu không tìm thấy username, lấy subject ID
                if (!string.IsNullOrWhiteSpace(claimSub))
                {
                    _logger.LogInformation("[DEBUG Username] Using sub as username: {Sub}", claimSub);
                    return claimSub;
                }

                _logger.LogWarning("[DEBUG Username] No username found in any claims!");
            }
            else
            {
                _logger.LogInformation("[DEBUG Username] User is not authenticated. Returning null.");
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