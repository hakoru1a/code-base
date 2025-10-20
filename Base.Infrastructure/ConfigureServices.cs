using Base.Infrastructure.Persistence;
using Base.Infrastructure.Repositories;
using Base.Domain.Interfaces;
using Infrastructure.DatabaseProviders;
using Infrastructure.Extensions;
using Common.Logging;
using Contracts.Common.Interface;
using Shared.Configurations.Database;
using Contracts.Services;
using Infrastructure.Services;
using Infrastructure.Common;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure
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

            // Configure DbContext based on provider type
            ConfigureDatabase(services, databaseProvider, databaseSettings.ConnectionStrings);

            // Log the selected database provider
            Console.WriteLine($"Using Database Provider: {databaseProvider.ProviderName}");

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
                services.AddScoped<IProductRedisRepository, ProductRedisRepository>();
            }

            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            services.AddScoped(typeof(ISMTPEmailServices), typeof(SMTTEmailServices));

            // ??ng ky c? Mediator va MassTransit ?? co th? s? d?ng c? 2
            // Mediator: X? ly domain events trong cung application
            // MassTransit: G?i messages ??n cac services khac
            // C?u hinh MassTransit
            //services.AddMassTransit(x =>
            //{
            //    x.UsingRabbitMq((context, cfg) =>
            //    {
            //        cfg.Host("localhost", "/", h =>
            //        {
            //            h.Username("guest");
            //            h.Password("guest");
            //        });
            //        cfg.ConfigureEndpoints(context);
            //    });
            //});

            services.ConfigureSwagger();
            return services;
        }

        private static void ConfigureDatabase(IServiceCollection services, IDatabaseProvider provider, string connectionString)
        {
            var migrationsAssembly = typeof(BaseContext).Assembly.FullName!;

            switch (provider)
            {
                case global::Infrastructure.DatabaseProviders.MySqlDatabaseProvider mysqlProvider:
                    mysqlProvider.ConfigureDbContext<BaseContext>(services, connectionString, migrationsAssembly);
                    break;

                case global::Infrastructure.DatabaseProviders.OracleDatabaseProvider oracleProvider:
                    oracleProvider.ConfigureDbContext<BaseContext>(services, connectionString, migrationsAssembly);
                    break;

                case global::Infrastructure.DatabaseProviders.PostgreSqlDatabaseProvider postgresProvider:
                    postgresProvider.ConfigureDbContext<BaseContext>(services, connectionString, migrationsAssembly);
                    break;

                default:
                    throw new NotSupportedException($"Database provider {provider.ProviderName} is not supported.");
            }
        }

        private static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ecom API",
                    Version = "v1",
                    Description = "E-commerce API for Product Management",  // Added meaningful description
                    Contact = new OpenApiContact
                    {
                        Name = "Chuong Dang",
                        Email = "hakoru1a@gmail.com",
                    }
                });
            });
        }

    }
}
