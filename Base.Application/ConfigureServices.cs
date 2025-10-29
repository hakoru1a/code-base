using System.Reflection;
using AutoMapper;
using Base.Infrastructure.Interfaces;
using Base.Infrastructure.Repositories;
using Base.Application.Common;
using Contracts.Common.Interface;
using FluentValidation;
using Infrastructure.Common;
using Infrastructure.Common.Behavior;
using Infrastructure.Common.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddSingleton(AddMapper())
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddScoped<ISerializeService, SerializeService>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(UnhandledExceptionBehaviour<,>))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(PerformanceBehaviour<,>))
            .AddTransient(serviceType: typeof(IPipelineBehavior<,>), implementationType: typeof(ValidationBehaviour<,>));
    private static IMapper AddMapper()
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });

        return mapperConfig.CreateMapper();

    }
}
