using Constracts.Common.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.DatabaseProviders
{
    public class OracleDatabaseProvider : IDatabaseProvider
    {
        public string ProviderName => "Oracle";

        public void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            // Generic implementation - will be called from the consuming project with proper context type
        }

        public void ConfigureMigrations(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseOracle(connectionString);
        }

        public void ConfigureDbContext<TContext>(IServiceCollection services, string connectionString, string? migrationsAssembly = null)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
                options.UseOracle(
                    connectionString,
                    builder =>
                    {
                        if (!string.IsNullOrEmpty(migrationsAssembly))
                            builder.MigrationsAssembly(migrationsAssembly);
                    })
            );
        }
    }
}

