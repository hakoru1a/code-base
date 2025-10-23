using AutoMapper;
using Common.Logging;
using FluentValidation;
using Infrastructure.Common.Behavior;
using Infrastructure.Policies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Generate.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register Validation Pipeline Behavior
            services.AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(PerformanceBehaviour<,>));
            services.AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}

