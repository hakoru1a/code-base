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

            // TODO: Register service-specific repositories as features are developed
            // services.AddScoped<IEntityRepository, EntityRepository>();

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
