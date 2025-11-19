using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Base.API.Filters;

namespace Base.API.Extensions
{
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Config();
            return services;
        }

        private static void Config(this IServiceCollection services)
        {
            var emailSettings = services.GetOptions<SMTPEmailSettings>(sectionName: nameof(SMTPEmailSettings));
            services.AddSingleton(emailSettings);
        }

        internal static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks();
            return services;
        }

        /// <summary>
        /// Add API versioning configuration
        /// </summary>
        internal static IServiceCollection AddBaseApiVersioning(this IServiceCollection services)
        {
            // API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });

            return services;
        }

        /// <summary>
        /// Add Swagger configuration with versioning support
        /// </summary>
        internal static IServiceCollection AddBaseSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            return services;
        }
    }

    /// <summary>
    /// Configure Swagger options using IConfigureOptions pattern
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Version = description.ApiVersion.ToString(),
                    Title = "Base API",
                    Description = "ASP.NET Core Web API for Base service operations",
                    Contact = new OpenApiContact
                    {
                        Name = "Base API Team",
                        Email = "support@base.com"
                    }
                });
            }

            // Add x-api-version header parameter for all operations
            options.AddSecurityDefinition("ApiVersion", new OpenApiSecurityScheme
            {
                Description = "API Version Header",
                Name = "x-api-version",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey"
            });

            options.OperationFilter<ApiVersionOperationFilter>();
        }
    }
}
