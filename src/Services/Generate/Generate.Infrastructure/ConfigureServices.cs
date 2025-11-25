using Generate.Infrastructure.Persistences;
using Generate.Infrastructure.Repositories;
using Generate.Application.Contracts.Persistence;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Generate.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add common infrastructure (Database + Redis)
            services.AddCommonInfrastructure<GenerateContext>(configuration);

            // Register service-specific repositories
< FOREACH >
    < LOOP > Tables </ LOOP >
    < CONTENT > services.AddScoped < I@@@Table.DisplayName @@@Repository, @@@Table.DisplayName @@@Repository> ();</ CONTENT >
</ FOREACH >

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
