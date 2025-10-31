using Contracts.Common.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.DatabaseProviders
{
    public class MySqlDatabaseProvider : IDatabaseProvider
    {
        public string ProviderName => "MySQL";

        public void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            // Generic implementation - will be called from the consuming project with proper context type
        }

        public void ConfigureMigrations(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }

        public void ConfigureDbContext<TContext>(IServiceCollection services, string connectionString, string? migrationsAssembly = null)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    builder =>
                    {
                        if (!string.IsNullOrEmpty(migrationsAssembly))
                            builder.MigrationsAssembly(migrationsAssembly);

                        // Enable connection resiliency
                        builder.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null
                        );

                        // Set command timeout
                        builder.CommandTimeout(30);
                    });

                // Enable lazy loading proxies
                options.UseLazyLoadingProxies();

                // Performance optimizations
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });
        }
    }
}

