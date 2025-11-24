using Auth.Application.Interfaces;
using Auth.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;

namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for Auth-specific services
/// </summary>
public static class AuthServicesExtensions
{
    /// <summary>
    /// Add authentication services with resilience policies
    /// </summary>
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IPkceService, PkceService>();
        services.AddScoped<ISessionManager, SessionManager>();

        services.AddHttpClient<IOAuthClient, OAuthClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }
}












