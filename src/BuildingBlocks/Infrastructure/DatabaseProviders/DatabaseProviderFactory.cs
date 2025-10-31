using Contracts.Common.Interface;
using Microsoft.Extensions.Configuration;
using System;

namespace Infrastructure.DatabaseProviders
{
    public static class DatabaseProviderFactory
    {
        public static IDatabaseProvider CreateProvider(IConfiguration configuration)
        {
            var providerName = configuration["DatabaseSettings:DBProvider"] ?? "MySQL";

            return providerName.ToUpperInvariant() switch
            {
                "MYSQL" => new MySqlDatabaseProvider(),
                "ORACLE" => new OracleDatabaseProvider(),
                "POSTGRESQL" => new PostgreSqlDatabaseProvider(),
                "POSTGRES" => new PostgreSqlDatabaseProvider(),
                _ => throw new NotSupportedException($"Database provider '{providerName}' is not supported. Supported providers: MySQL, Oracle, PostgreSQL")
            };
        }
    }
}

