using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Infrastructure.Filters;

namespace Infrastructure.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, string apiTitle = "API", string apiDescription = "A Web API service", string contactName = "API Team", string contactEmail = "support@api.com")
        {
            // Register the Swagger configuration options
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(provider => 
                new ConfigureSwaggerOptions(
                    provider.GetRequiredService<IApiVersionDescriptionProvider>(),
                    apiTitle,
                    apiDescription,
                    contactName,
                    contactEmail));
            
            services.AddSwaggerGen(options =>
            {
                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Add operation filter for API versioning
                options.OperationFilter<ApiVersionOperationFilter>();

                // Enable XML comments if available
                var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, string routePrefix = "swagger")
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = routePrefix;
            });

            return app;
        }

        // Configure Swagger options using IConfigureOptions pattern
        public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;
            private readonly string _apiTitle;
            private readonly string _apiDescription;
            private readonly string _contactName;
            private readonly string _contactEmail;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, string apiTitle, string apiDescription, string contactName, string contactEmail)
            {
                _provider = provider;
                _apiTitle = apiTitle;
                _apiDescription = apiDescription;
                _contactName = contactName;
                _contactEmail = contactEmail;
            }

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = _apiTitle,
                        Description = _apiDescription,
                        Contact = new OpenApiContact
                        {
                            Name = _contactName,
                            Email = _contactEmail
                        }
                    });
                }
            }
        }
    }
}