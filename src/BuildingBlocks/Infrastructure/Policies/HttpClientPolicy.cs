using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Infrastructure.Policies
{
    public static class HttpClientPolicy
    {

        public static IHttpClientBuilder UseImmediateHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 3)
        {
            return builder.AddPolicyHandler(ImmediateHttpRetry(retryCount));
        }

        public static IHttpClientBuilder UseLinearHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 3, int waitSeconds = 3)
        {
            return builder.AddPolicyHandler(LinearHttpRetry(retryCount, waitSeconds));
        }

        public static IHttpClientBuilder UseExponentialHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 3)
        {
            return builder.AddPolicyHandler(ExponentialHttpRetry(retryCount));
        }

        public static IHttpClientBuilder UseCircuitBreakerPolicy(this IHttpClientBuilder builder, int eventsBeforeBreaking = 3, int durationOfBreakSeconds = 30)
        {
            return builder.AddPolicyHandler(CircuitBreakerPolicy(eventsBeforeBreaking, durationOfBreakSeconds));
        }

        private static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(int eventsBeforeBreaking, int durationOfBreakSeconds)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(eventsBeforeBreaking, TimeSpan.FromSeconds(durationOfBreakSeconds));
        }

        public static IHttpClientBuilder ConfigureTimeoutPolicy(this IHttpClientBuilder builder, int seconds = 5)
        {
            return builder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(seconds));
        }

        private static IAsyncPolicy<HttpResponseMessage> ImmediateHttpRetry(int retryCount) =>
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .Or<TimeoutRejectedException>()
                        .RetryAsync(retryCount: 3, onRetry: (exception, retryCount, context) =>
                        {
                            Log.Error(messageTemplate: $"Retry {retryCount} of {context.PolicyKey} at " +
                                     $"{context.OperationKey}, due to: {exception.Exception.Message}");
                        });

        private static IAsyncPolicy<HttpResponseMessage> LinearHttpRetry(int retryCount, int waitSeconds) =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount,
                    _ => TimeSpan.FromSeconds(waitSeconds),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception.Exception.Message}");
                    });

        private static IAsyncPolicy<HttpResponseMessage> ExponentialHttpRetry(int retryCount) =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception.Exception.Message}");
                    });
    }
}
