using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.DatabaseProviders;
using Infrastructure.Extensions;
using Shared.Configurations.Database;
using StackExchange.Redis;
using Infrastructure.Common.Repository;
using Contracts.Common.Interface;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Generic Infrastructure services configuration
    /// </summary>
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddDatabaseInfrastructure<TContext>(this IServiceCollection services, IConfiguration configuration) 
            where TContext : DbContext
        {
            // Get Database Settings
            var databaseSettings = configuration.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));

            if (string.IsNullOrEmpty(databaseSettings?.ConnectionStrings))
            {
                throw new InvalidOperationException($"Database connection string is not configured for {typeof(TContext).Name}. Please configure DatabaseSettings:ConnectionStrings in appsettings.json");
            }

            // Database Provider Configuration
            var databaseProvider = DatabaseProviderFactory.CreateProvider(configuration);
            var migrationsAssembly = typeof(TContext).Assembly.FullName!;

            // Configure DbContext based on provider type
            switch (databaseProvider)
            {
                case MySqlDatabaseProvider mysqlProvider:
                    mysqlProvider.ConfigureDbContext<TContext>(services, databaseSettings.ConnectionStrings, migrationsAssembly);
                    break;

                case OracleDatabaseProvider oracleProvider:
                    oracleProvider.ConfigureDbContext<TContext>(services, databaseSettings.ConnectionStrings, migrationsAssembly);
                    break;

                case PostgreSqlDatabaseProvider postgresProvider:
                    postgresProvider.ConfigureDbContext<TContext>(services, databaseSettings.ConnectionStrings, migrationsAssembly);
                    break;

                default:
                    throw new NotSupportedException($"Database provider {databaseProvider.ProviderName} is not supported.");
            }

            // Register Unit of Work
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            return services;
        }

        public static IServiceCollection AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis Configuration
            var cacheSettings = configuration.GetOptions<CacheSettings>(nameof(CacheSettings));
            
            if (!string.IsNullOrEmpty(cacheSettings?.ConnectionStrings))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configurationOptions = ConfigurationOptions.Parse(cacheSettings.ConnectionStrings);
                    configurationOptions.AbortOnConnectFail = false;
                    configurationOptions.ConnectTimeout = 5000;
                    configurationOptions.SyncTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });
                services.AddScoped<IRedisRepository, RedisRepository>();
            }

            return services;
        }

        public static IServiceCollection AddCommonInfrastructure<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            return services
                .AddDatabaseInfrastructure<TContext>(configuration)
                .AddRedisInfrastructure(configuration);
        }
    }
}