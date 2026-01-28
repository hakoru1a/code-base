using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;
using TLBIOMASS.Infrastructure.Repositories;

namespace TLBIOMASS.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCommonInfrastructure<TLBIOMASSContext>(configuration);

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IReceiverRepository, ReceiverRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<ILandownerRepository, LandownerRepository>();
            services.AddScoped<IMaterialRegionRepository, MaterialRegionRepository>();

            return services;
        }
    }
}
