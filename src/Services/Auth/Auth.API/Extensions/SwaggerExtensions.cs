using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Auth.API.Filters;

namespace Auth.API.Extensions;

/// <summary>
/// Extension methods for Swagger configuration
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configure Swagger documentation for Auth API
    /// </summary>
    public static IServiceCollection AddAuthSwagger(this IServiceCollection services)
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

        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }

    /// <summary>
    /// Use Swagger UI for development environment
    /// </summary>
    public static IApplicationBuilder UseAuthSwagger(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        app.UseSwagger();

        if (environment.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                        $"Auth API {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = "swagger";
            });
        }

        return app;
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
                    Title = "Auth API",
                    Description = "Authentication service xử lý OAuth 2.0/OIDC với Keycloak"
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






