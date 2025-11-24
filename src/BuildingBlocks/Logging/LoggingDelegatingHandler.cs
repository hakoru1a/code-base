using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;

namespace Common.Logging
{
    public class LoggingDelegatingHandler : DelegatingHandler // To log out http call outbound
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Thêm correlation ID vào outbound request header
            AddCorrelationIdToRequest(request);

            try
            {
                _logger.LogInformation(message: "Sending request to {Url} - Method {Method} - Version {Version}",
                    request.RequestUri,
                    request.Method,
                    request.Version);

                var response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(message: "Received a success response from {Url}",
                        response.RequestMessage?.RequestUri);
                }
                else
                {
                    _logger.LogWarning(message: "Received a non-success status code {StatusCode} from {Url}",
                        (int)response.StatusCode, response.RequestMessage?.RequestUri);
                }

                return response;
            }
            catch (HttpRequestException ex)
                when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
            {
                var hostWithPort = request.RequestUri?.IsDefaultPort == true
                    ? request.RequestUri.DnsSafeHost
                    : $"{request.RequestUri?.DnsSafeHost}:{request.RequestUri?.Port}";

                _logger.LogCritical(ex, message: "Unable to connect to {Host}. Please check the " +
                    "configuration/settings to ensure the correct URL for the service " +
                    "has been configured", hostWithPort);

                return new HttpResponseMessage(HttpStatusCode.BadGateway)
                {
                    RequestMessage = request
                };
            }
        }

        /// <summary>
        /// Thêm correlation ID từ current HTTP request vào outbound request header
        /// Điều này đảm bảo correlation ID được truyền qua toàn bộ microservices
        /// </summary>
        private void AddCorrelationIdToRequest(HttpRequestMessage request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Thử lấy correlation ID từ các nguồn khác nhau
            string? correlationId = null;

            // 1. Từ HttpContext.Items (được set bởi LoggingContextMiddleware)
            if (httpContext.Items.TryGetValue("CorrelationId", out var contextCorrelationId))
            {
                correlationId = contextCorrelationId?.ToString();
            }

            // 2. Từ request header (fallback)
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            }

            // 3. Từ TraceIdentifier (fallback cuối cùng)
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = httpContext.TraceIdentifier;
            }

            // Thêm vào outbound request header nếu có
            if (!string.IsNullOrWhiteSpace(correlationId) && 
                !request.Headers.Contains("X-Correlation-Id"))
            {
                request.Headers.Add("X-Correlation-Id", correlationId);
            }
        }
    }
}
