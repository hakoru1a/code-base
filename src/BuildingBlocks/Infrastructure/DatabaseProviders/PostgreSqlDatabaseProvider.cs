using Contracts.Common.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.DatabaseProviders
{
    public class PostgreSqlDatabaseProvider : IDatabaseProvider
    {
        public string ProviderName => "PostgreSQL";

        public void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            // Generic implementation - will be called from the consuming project with proper context type
        }

        public void ConfigureMigrations(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseNpgsql(connectionString);
        }

        public void ConfigureDbContext<TContext>(IServiceCollection services, string connectionString, string? migrationsAssembly = null)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    builder =>
                    {
                        if (!string.IsNullOrEmpty(migrationsAssembly))
                            builder.MigrationsAssembly(migrationsAssembly);
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

