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
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Extension method for HttpClient to post JSON data
        /// </summary>
        public static Task<HttpResponseMessage> PostAsJson<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return httpClient.PostAsync(url, content);
        }

        /// <summary>
        /// Extension method for HttpResponseMessage to read content as typed object
        /// </summary>
        public static async Task<T> ReadContentAs<T>(this HttpResponseMessage response)
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
        /// Add a named HttpClient with resilience policies and logging
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="name">Name of the HttpClient</param>
        /// <param name="baseAddress">Base address for the HttpClient</param>
        /// <param name="timeoutSeconds">Timeout in seconds (default: 30)</param>
        /// <param name="retryCount">Number of retries (default: 3)</param>
        /// <param name="circuitBreakerEvents">Events before circuit breaker opens (default: 5)</param>
        /// <param name="circuitBreakerDuration">Duration of circuit break in seconds (default: 30)</param>
        /// <returns>IHttpClientBuilder for further configuration</returns>
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
        /// Add multiple named HttpClients from configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="clientConfigurations">Dictionary of client configurations (name -> baseAddress)</param>
        /// <param name="timeoutSeconds">Timeout in seconds (default: 30)</param>
        /// <param name="retryCount">Number of retries (default: 3)</param>
        /// <param name="circuitBreakerEvents">Events before circuit breaker opens (default: 5)</param>
        /// <param name="circuitBreakerDuration">Duration of circuit break in seconds (default: 30)</param>
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
