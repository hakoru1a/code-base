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
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho Infrastructure Layer - Database, Redis, Unit of Work
    /// 
    /// MỤC ĐÍCH:
    /// - Cấu hình Database (MySQL, PostgreSQL, Oracle) với DbContext
    /// - Cấu hình Redis cache với ConnectionMultiplexer
    /// - Đăng ký Unit of Work pattern cho transaction management
    /// - Hỗ trợ multi-database provider từ configuration
    /// 
    /// SỬ DỤNG:
    /// 1. Database only:
    ///    services.AddDatabaseInfrastructure<OrderDbContext>(configuration);
    /// 
    /// 2. Database + Redis:
    ///    services.AddCommonInfrastructure<OrderDbContext>(configuration);
    /// 
    /// 3. Redis only:
    ///    services.AddRedisInfrastructure(configuration);
    /// 
    /// CONFIGURATION (appsettings.json):
    /// {
    ///   "DatabaseSettings": {
    ///     "DBProvider": "MySQL",  // hoặc "PostgreSQL", "Oracle"
    ///     "ConnectionStrings": "Server=localhost;Database=OrderDb;..."
    ///   },
    ///   "CacheSettings": {
    ///     "ConnectionStrings": "localhost:6379,password=secret"
    ///   }
    /// }
    /// 
    /// IMPACT:
    /// + Unit of Work: Transaction safety, đảm bảo data consistency
    /// + Multi-Provider: Dễ dàng switch giữa MySQL, PostgreSQL, Oracle
    /// + Redis Cache: Giảm database load, tăng response time
    /// + Repository Pattern: Abstraction layer, dễ test và mock
    /// - Connection Pool: Cần monitor connection pool, tránh exhaustion
    /// - Redis Dependency: Hệ thống phụ thuộc vào Redis availability
    /// </summary>
    public static class InfrastructureExtensions
    {
        /// <summary>
        /// Cấu hình Database infrastructure với support cho nhiều providers
        /// 
        /// CÁCH DÙNG:
        /// services.AddDatabaseInfrastructure<ProductDbContext>(configuration);
        /// 
        /// PROVIDERS HỖ TRỢ:
        /// - MySQL (Pomelo.EntityFrameworkCore.MySql)
        /// - PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
        /// - Oracle (Oracle.EntityFrameworkCore)
        /// 
        /// TỰ ĐỘNG ĐĂNG KÝ:
        /// - DbContext với connection string từ config
        /// - Unit of Work pattern (IUnitOfWork<TContext>)
        /// - Migration assembly từ DbContext assembly
        /// </summary>
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

        /// <summary>
        /// Cấu hình Redis cache infrastructure
        /// 
        /// CÁCH DÙNG:
        /// services.AddRedisInfrastructure(configuration);
        /// 
        /// CONFIGURATION:
        /// {
        ///   "CacheSettings": {
        ///     "ConnectionStrings": "localhost:6379,password=secret,ssl=false"
        ///   }
        /// }
        /// 
        /// TỰ ĐỘNG ĐĂNG KÝ:
        /// - IConnectionMultiplexer (Singleton): Redis connection pool
        /// - IRedisRepository (Scoped): Repository để thao tác với Redis
        /// 
        /// FEATURES:
        /// - AbortOnConnectFail = false: Không crash app nếu Redis down
        /// - ConnectTimeout & SyncTimeout = 5s: Tránh hang application
        /// 
        /// PHÙ HỢP: Session storage, distributed cache, rate limiting, pub/sub
        /// </summary>
        public static IServiceCollection AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis Configuration
            var cacheSettings = configuration.GetOptions<CacheSettings>(nameof(CacheSettings));

            if (!string.IsNullOrEmpty(cacheSettings?.ConnectionStrings))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configurationOptions = ConfigurationOptions.Parse(cacheSettings.ConnectionStrings);
                    
                    // Add password if provided
                    if (!string.IsNullOrEmpty(cacheSettings.Password))
                    {
                        configurationOptions.Password = cacheSettings.Password;
                    }
                    
                    configurationOptions.AbortOnConnectFail = false;
                    configurationOptions.ConnectTimeout = 5000;
                    configurationOptions.SyncTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });
                services.AddScoped<IRedisRepository, RedisRepository>();
                
                // Register IDistributedCache for caching services (JWT claims, PKCE data, etc.)
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheSettings.ConnectionStrings;
                    if (!string.IsNullOrEmpty(cacheSettings.Password))
                    {
                        var configOptions = ConfigurationOptions.Parse(cacheSettings.ConnectionStrings);
                        configOptions.Password = cacheSettings.Password;
                        options.ConfigurationOptions = configOptions;
                    }
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis not configured
                services.AddDistributedMemoryCache();
            }

            return services;
        }

        /// <summary>
        /// Cấu hình toàn bộ infrastructure: Database + Redis
        /// 
        /// CÁCH DÙNG:
        /// services.AddCommonInfrastructure<OrderDbContext>(configuration);
        /// 
        /// TƯƠNG ĐƯƠNG:
        /// services.AddDatabaseInfrastructure<OrderDbContext>(configuration)
        ///         .AddRedisInfrastructure(configuration);
        /// 
        /// PHÙ HỢP: Hầu hết các microservices cần cả database và cache
        /// </summary>
        public static IServiceCollection AddCommonInfrastructure<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            return services
                .AddDatabaseInfrastructure<TContext>(configuration)
                .AddRedisInfrastructure(configuration);
        }
    }
}