using ApiGateway.Configurations;
using Common.Logging;
using Infrastructure.Policies;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring HttpClient with policies and logging
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Add named HttpClients with logging and resilience policies
    /// </summary>
    public static IServiceCollection AddConfiguredHttpClients(
        this IServiceCollection services,
        ServicesOptions servicesOptions)
    {
        // Register LoggingDelegatingHandler for HTTP logging
        services.AddTransient<LoggingDelegatingHandler>();

        // HttpClient for AuthService
        services.AddHttpClient("AuthService", client =>
        {
            client.BaseAddress = new Uri(servicesOptions.AuthAPI.Url);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseExponentialHttpRetryPolicy(retryCount: 3)
        .UseCircuitBreakerPolicy(eventsBeforeBreaking: 5, durationOfBreakSeconds: 30)
        .ConfigureTimeoutPolicy(seconds: 30);

        // HttpClient for BaseAPI
        services.AddHttpClient("BaseAPI", client =>
        {
            client.BaseAddress = new Uri(servicesOptions.BaseAPI.Url);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseExponentialHttpRetryPolicy(retryCount: 3)
        .UseCircuitBreakerPolicy(eventsBeforeBreaking: 5, durationOfBreakSeconds: 30)
        .ConfigureTimeoutPolicy(seconds: 30);

        // HttpClient for GenerateAPI
        services.AddHttpClient("GenerateAPI", client =>
        {
            client.BaseAddress = new Uri(servicesOptions.GenerateAPI.Url);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseExponentialHttpRetryPolicy(retryCount: 3)
        .UseCircuitBreakerPolicy(eventsBeforeBreaking: 5, durationOfBreakSeconds: 30)
        .ConfigureTimeoutPolicy(seconds: 30);

        // Default HttpClient (fallback)
        services.AddHttpClient();

        return services;
    }
}





