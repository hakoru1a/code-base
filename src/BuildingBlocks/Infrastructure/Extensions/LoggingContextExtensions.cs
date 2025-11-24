using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for logging context middleware
    /// </summary>
    public static class LoggingContextExtensions
    {
        /// <summary>
        /// Thêm LoggingContextMiddleware vào pipeline
        /// Middleware này sẽ:
        /// - Tự động tạo correlation ID cho mỗi request
        /// - Extract username từ JWT claims
        /// - Thêm các thông tin này vào Serilog LogContext
        /// - Log request start/end với correlation ID và username
        /// 
        /// CÁCH SỬ DỤNG:
        /// app.UseLoggingContext(); // Đặt sau UseAuthentication() và trước UseAuthorization()
        /// 
        /// KẾT QUẢ:
        /// - Mọi log trong request đều có CorrelationId và Username
        /// - Dễ dàng filter log theo user hoặc correlation ID trong Kibana
        /// - Response header có X-Correlation-Id để client track
        /// </summary>
        public static IApplicationBuilder UseLoggingContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingContextMiddleware>();
        }
    }
}