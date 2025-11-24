using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Policies;
using Common.Logging;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho HttpClient - Hỗ trợ gọi HTTP API với retry policy và circuit breaker
    /// 
    /// MỤC ĐÍCH:
    /// - Cung cấp các helper methods để gọi HTTP APIs một cách dễ dàng
    /// - Tích hợp sẵn retry policy, circuit breaker, timeout để tăng độ resilient
    /// - Thêm logging cho tất cả HTTP calls để dễ dàng debug và monitor
    /// 
    /// SỬ DỤNG:
    /// 1. Đăng ký HttpClient với resilience:
    ///    services.AddNamedHttpClientWithResilience("ProductService", "https://api.product.com");
    /// 
    /// 2. Sử dụng trong code:
    ///    var client = _httpClientFactory.CreateClient("ProductService");
    ///    var response = await client.PostAsJson("/api/products", productDto);
    ///    var result = await response.ReadContentAs<ProductResponse>();
    /// 
    /// IMPACT:
    /// + Tăng độ tin cậy: Tự động retry khi gặp lỗi tạm thời (network issues)
    /// + Bảo vệ hệ thống: Circuit breaker ngăn chặn cascading failures
    /// + Dễ debug: Logging tất cả requests/responses qua LoggingDelegatingHandler
    /// + Performance: Timeout configuration tránh việc chờ requests quá lâu
    /// - Trade-off: Retry có thể làm tăng latency trong trường hợp lỗi
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Gửi POST request với JSON payload
        /// 
        /// SỬ DỤNG: await httpClient.PostAsJson("/api/users", userDto);
        /// </summary>
        public static Task<HttpResponseMessage> PostAsJson<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return httpClient.PostAsync(url, content);
        }

        /// <summary>
        /// Đọc response body và deserialize thành object
        /// 
        /// SỬ DỤNG: var user = await response.ReadContentAs<UserDto>();
        /// LƯU Ý: Tự động throw exception nếu response không success (4xx, 5xx)
        /// </summary>
        public static async Task<T?> ReadContentAs<T>(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // For boolean results, return true for success
                if (typeof(T) == typeof(bool))
                    return (T)(object)true;
                return default;
            }

            var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(dataAsString))
            {
                // For boolean results, return true for success
                if (typeof(T) == typeof(bool))
                    return (T)(object)true;
                return default;
            }

            return JsonSerializer.Deserialize<T>(dataAsString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            });
        }

        /// <summary>
        /// Đăng ký một named HttpClient với đầy đủ resilience patterns
        /// 
        /// CHỨC NĂNG:
        /// - Exponential Retry: Tự động retry với delay tăng dần khi gặp lỗi tạm thời (503, network timeout)
        /// - Circuit Breaker: Tự động ngắt kết nối khi service downstream liên tục lỗi, tránh overload
        /// - Timeout Policy: Giới hạn thời gian chờ mỗi request
        /// - Logging: Tự động log tất cả requests/responses để tracking
        /// 
        /// CÁCH DÙNG:
        /// services.AddNamedHttpClientWithResilience(
        ///     "OrderService", 
        ///     "https://order-api.com",
        ///     timeoutSeconds: 30,      // Timeout mỗi request
        ///     retryCount: 3,           // Retry tối đa 3 lần
        ///     circuitBreakerEvents: 5, // Mở circuit sau 5 lỗi liên tiếp
        ///     circuitBreakerDuration: 30 // Giữ circuit mở trong 30s
        /// );
        /// 
        /// SAU ĐÓ SỬ DỤNG:
        /// var client = _httpClientFactory.CreateClient("OrderService");
        /// var response = await client.GetAsync("/api/orders/123");
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="name">Tên của HttpClient (dùng để lấy ra sau này)</param>
        /// <param name="baseAddress">Base URL của service cần gọi</param>
        /// <param name="timeoutSeconds">Timeout tối đa cho mỗi request (mặc định: 30s)</param>
        /// <param name="retryCount">Số lần retry khi gặp lỗi (mặc định: 3 lần)</param>
        /// <param name="circuitBreakerEvents">Số lỗi liên tiếp trước khi mở circuit breaker (mặc định: 5 lần)</param>
        /// <param name="circuitBreakerDuration">Thời gian giữ circuit breaker ở trạng thái mở (mặc định: 30s)</param>
        /// <returns>IHttpClientBuilder để có thể config thêm nếu cần</returns>
        public static IHttpClientBuilder AddNamedHttpClientWithResilience(
            this IServiceCollection services,
            string name,
            string baseAddress,
            int timeoutSeconds = 30,
            int retryCount = 3,
            int circuitBreakerEvents = 5,
            int circuitBreakerDuration = 30)
        {
            // Register LoggingDelegatingHandler if not already registered
            services.AddTransient<LoggingDelegatingHandler>();

            return services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .UseExponentialHttpRetryPolicy(retryCount: retryCount)
            .UseCircuitBreakerPolicy(eventsBeforeBreaking: circuitBreakerEvents, durationOfBreakSeconds: circuitBreakerDuration)
            .ConfigureTimeoutPolicy(seconds: timeoutSeconds);
        }

        /// <summary>
        /// Đăng ký nhiều HttpClients cùng lúc từ configuration
        /// 
        /// CÁCH DÙNG:
        /// var clientConfigs = new Dictionary<string, string>
        /// {
        ///     ["ProductService"] = "https://product-api.com",
        ///     ["OrderService"] = "https://order-api.com",
        ///     ["UserService"] = "https://user-api.com"
        /// };
        /// services.AddMultipleHttpClientsWithResilience(clientConfigs);
        /// 
        /// HOẶC từ appsettings.json:
        /// {
        ///   "ExternalServices": {
        ///     "ProductService": "https://product-api.com",
        ///     "OrderService": "https://order-api.com"
        ///   }
        /// }
        /// var configs = configuration.GetSection("ExternalServices").Get<Dictionary<string, string>>();
        /// services.AddMultipleHttpClientsWithResilience(configs);
        /// 
        /// LỢI ÍCH: Tất cả HttpClients đều có cùng resilience configuration, dễ maintain
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="clientConfigurations">Dictionary ánh xạ tên client -> base URL</param>
        /// <param name="timeoutSeconds">Timeout cho tất cả clients (mặc định: 30s)</param>
        /// <param name="retryCount">Số lần retry cho tất cả clients (mặc định: 3 lần)</param>
        /// <param name="circuitBreakerEvents">Số lỗi trước khi mở circuit breaker (mặc định: 5 lần)</param>
        /// <param name="circuitBreakerDuration">Thời gian circuit breaker mở (mặc định: 30s)</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMultipleHttpClientsWithResilience(
            this IServiceCollection services,
            IDictionary<string, string> clientConfigurations,
            int timeoutSeconds = 30,
            int retryCount = 3,
            int circuitBreakerEvents = 5,
            int circuitBreakerDuration = 30)
        {
            foreach (var (name, baseAddress) in clientConfigurations)
            {
                services.AddNamedHttpClientWithResilience(
                    name,
                    baseAddress,
                    timeoutSeconds,
                    retryCount,
                    circuitBreakerEvents,
                    circuitBreakerDuration);
            }

            // Add default HttpClient
            services.AddHttpClient();

            return services;
        }
    }
}
