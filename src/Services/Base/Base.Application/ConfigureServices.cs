using System.Reflection;
using Base.Infrastructure.Interfaces;
using Base.Infrastructure.Repositories;
using Base.Application.Common;
using Base.Application.Feature.Product.Services;
using Contracts.Common.Interface;
using FluentValidation;
using Infrastructure.Common;
using Infrastructure.Common.Behavior;
using Infrastructure.Common.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MapsterMapper;

namespace Base.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Configure Mapster mappings
        MapsterConfig.ConfigureMappings();
        
        return services
            .AddSingleton(TypeAdapterConfig.GlobalSettings)
            .AddScoped<IMapper, ServiceMapper>()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddScoped<ISerializeService, SerializeService>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProductPolicyService, ProductPolicyService>()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(UnhandledExceptionBehaviour<,>))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(PerformanceBehaviour<,>))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(ValidationBehaviour<,>));
    }
}
