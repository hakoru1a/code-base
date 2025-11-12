using Auth.Domain.Configurations;
using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using StackExchange.Redis;

namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for Redis configuration
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Add Redis connection and repository
    /// </summary>
    public static IServiceCollection AddRedisConfiguration(
        this IServiceCollection services,
        AuthSettings authSettings)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(authSettings.ConnectionStrings);
            configuration.AbortOnConnectFail = false;
            configuration.ConnectRetry = 3;
            configuration.ConnectTimeout = 5000;

            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddScoped<IRedisRepository, RedisRepository>();

        return services;
    }
}

