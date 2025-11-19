using FluentValidation;
using Infrastructure.Common.Behavior;
using Infrastructure.Policies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Mapster;
using MapsterMapper;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Generic Application services configuration for all services
    /// </summary>
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddGenericApplicationServices(this IServiceCollection services, Assembly? assembly = null)
        {
            var targetAssembly = assembly ?? Assembly.GetCallingAssembly();

            // Register Mapster - High performance mapping
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            services.AddScoped<IMapper, ServiceMapper>();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(targetAssembly));

            // Register FluentValidation
            services.AddValidatorsFromAssembly(targetAssembly);

            // Register Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }

        public static IServiceCollection AddApplicationServicesWithMapsterConfig<TMapsterConfig>(this IServiceCollection services, Assembly? assembly = null) 
            where TMapsterConfig : class, new()
        {
            var targetAssembly = assembly ?? Assembly.GetCallingAssembly();

            // Configure Mapster with specific config class
            var mapsterConfig = new TMapsterConfig();
            if (mapsterConfig is IRegister register)
            {
                register.Register(TypeAdapterConfig.GlobalSettings);
            }

            return services.AddGenericApplicationServices(targetAssembly);
        }
    }
}