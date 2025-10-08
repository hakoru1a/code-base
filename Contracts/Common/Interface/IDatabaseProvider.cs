using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Constracts.Common.Interface
{
    public interface IDatabaseProvider
    {
        string ProviderName { get; }
        void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString);
        void ConfigureMigrations(DbContextOptionsBuilder options, string connectionString);
    }
}
