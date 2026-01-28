using TLBIOMASS.Infrastructure.Persistences;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TLBIOMASS.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add common infrastructure (Database + Redis)
            services.AddCommonInfrastructure<TLBIOMASSContext>(configuration);

            services.AddScoped<TLBIOMASS.Domain.Receivers.Interfaces.IReceiverRepository, TLBIOMASS.Infrastructure.Repositories.ReceiverRepository>();
            services.AddScoped<TLBIOMASS.Domain.Agencies.Interfaces.IAgencyRepository, TLBIOMASS.Infrastructure.Repositories.AgencyRepository>();
            services.AddScoped<TLBIOMASS.Domain.Materials.Interfaces.IMaterialRepository, TLBIOMASS.Infrastructure.Repositories.MaterialRepository>();
            services.AddScoped<TLBIOMASS.Domain.Landowners.Interfaces.ILandownerRepository, TLBIOMASS.Infrastructure.Repositories.LandownerRepository>();
            services.AddScoped<TLBIOMASS.Domain.MaterialRegions.Interfaces.IMaterialRegionRepository, TLBIOMASS.Infrastructure.Repositories.MaterialRegionRepository>();

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
    }
}
