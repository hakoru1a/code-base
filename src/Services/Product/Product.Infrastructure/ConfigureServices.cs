using Product.Infrastructure.Persistences;
using Product.Infrastructure.Repositories;
using Product.Infrastructure.Interfaces;
using Infrastructure.DatabaseProviders;
using Infrastructure.Extensions;
using Contracts.Common.Interface;
using Shared.Configurations.Database;
using Infrastructure.Common;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using MassTransit;
using System;

namespace Product.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Get Database Settings using GetOptions
            var databaseSettings = services.GetOptions<DatabaseSettings>("DatabaseSettings");

            if (string.IsNullOrEmpty(databaseSettings?.ConnectionStrings))
            {
                throw new InvalidOperationException("Database connection string is not configured. Please configure DatabaseSettings:ConnectionStrings in appsettings.json");
            }

            // Database Provider Configuration
            var databaseProvider = DatabaseProviderFactory.CreateProvider(configuration);

            databaseProvider.ConfigureDbContext(services, configuration, databaseSettings.ConnectionStrings);
            // Configure DbContext based on provider type
            ConfigureDatabase(services, databaseProvider, databaseSettings.ConnectionStrings);

            // Redis Configuration using GetOptions
            var cacheSettings = services.GetOptions<CacheSettings>("CacheSettings");
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

            // Register repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IAttributeDefRepository, AttributeDefRepository>();
            services.AddScoped<IProductVariantAttributeRepository, ProductVariantAttributeRepository>();

            // Register Unit of Work
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            // Configure MassTransit with RabbitMQ (optional - uncomment if needed)
            // services.AddMassTransit(x =>
            // {
            //     x.UsingRabbitMq((context, cfg) =>
            //     {
            //         var rabbitMQSettings = configuration.GetSection("RabbitMQSettings");
            //         cfg.Host(rabbitMQSettings["Host"], Convert.ToUInt16(rabbitMQSettings["Port"]), "/", h =>
            //         {
            //             h.Username(rabbitMQSettings["Username"]);
            //             h.Password(rabbitMQSettings["Password"]);
            //         });
            //         cfg.ConfigureEndpoints(context);
            //     });
            // });

            return services;
        }

        private static void ConfigureDatabase(IServiceCollection services, IDatabaseProvider provider, string connectionString)
        {
            var migrationsAssembly = typeof(ProductContext).Assembly.FullName!;

            switch (provider)
            {
                case global::Infrastructure.DatabaseProviders.MySqlDatabaseProvider mysqlProvider:
                    mysqlProvider.ConfigureDbContext<ProductContext>(services, connectionString, migrationsAssembly);
                    break;

                case global::Infrastructure.DatabaseProviders.OracleDatabaseProvider oracleProvider:
                    oracleProvider.ConfigureDbContext<ProductContext>(services, connectionString, migrationsAssembly);
                    break;

                case global::Infrastructure.DatabaseProviders.PostgreSqlDatabaseProvider postgresProvider:
                    postgresProvider.ConfigureDbContext<ProductContext>(services, connectionString, migrationsAssembly);
                    break;

                default:
                    throw new NotSupportedException($"Database provider {provider.ProviderName} is not supported.");
            }
        }

    }
}