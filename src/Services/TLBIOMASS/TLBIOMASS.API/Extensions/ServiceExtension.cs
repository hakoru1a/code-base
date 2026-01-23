using Infrastructure.Extensions;
using Infrastructure.Filters;
using Shared.Configurations;
using Shared.Configurations.Database;
using Microsoft.Extensions.Options;
using TLBIOMASS.Application.Common.Mappings;
using System.Reflection;

namespace TLBIOMASS.API.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddControllerServices()
                .AddConfigurationSettings(configuration)
                .AddInfrastructureModules()
                .AddApplicationServicesIntegrated() 
                .AddCommonServices();
        }

        private static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(ValidateModelStateFilter));
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                    options.JsonSerializerOptions.WriteIndented = false;
                });

            return services;
        }

        private static IServiceCollection AddInfrastructureModules(this IServiceCollection services)
        {
            return services
                .AddApiVersioningConfiguration()
                .AddSwaggerConfiguration("TLBIOMASS API", "An ASP.NET Core Web API for managing TLBIOMASS entities", "TLBIOMASS API Team", "support@tlbiomass.com")
                .AddCorsConfiguration()
                .AddHealthCheckConfiguration();
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            return services
                .AddHttpContextAccessor()
                .AddEndpointsApiExplorer();
        }

        private static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            // Use GetOptions extension method with IConfiguration
            services.AddSingleton(configuration.GetOptions<DatabaseSettings>(nameof(DatabaseSettings)));
            services.AddSingleton(configuration.GetOptions<CacheSettings>(nameof(CacheSettings)));
            services.AddSingleton(configuration.GetOptions<JwtSettings>(nameof(JwtSettings)));

            return services;
        }

        private static IServiceCollection AddApplicationServicesIntegrated(this IServiceCollection services)
        {
            // Configure Mapster mappings first
            MapsterConfig.ConfigureMappings();
            
            // Use generic application services from Infrastructure with Application assembly
            var applicationAssembly = typeof(TLBIOMASS.Application.Common.Mappings.MapsterConfig).Assembly;
            return services.AddGenericApplicationServices(applicationAssembly);
        }
    }
}

