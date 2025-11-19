using Infrastructure.Extensions;
using Infrastructure.Filters;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Shared.Configurations;
using Shared.Configurations.Database;
using Swashbuckle.AspNetCore.SwaggerGen;
using Generate.API.Filters;

namespace Generate.API.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Controllers with JSON options
            services.AddControllers(options =>
                {
                    // Add custom validation filter
                    options.Filters.Add<ValidateModelStateFilter>();
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Suppress default ModelState validation error response
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.WriteIndented = false;
                });

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

            // Add Configuration Settings
            services.AddConfigurationSettings(configuration);

            // Add Swagger
            services.AddSwaggerConfiguration();

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            // Add Health Checks
            services.AddHealthChecks();

            // Add HttpContextAccessor
            services.AddHttpContextAccessor();

            // Add EndpointsApiExplorer
            services.AddEndpointsApiExplorer();

            return services;
        }

        private static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            // Read and register configuration settings
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            services.AddSingleton(databaseSettings);

            var cacheSettings = services.GetOptions<CacheSettings>(nameof(CacheSettings));
            services.AddSingleton(cacheSettings);

            var jwtSettings = services.GetOptions<JwtSettings>(nameof(JwtSettings));
            services.AddSingleton(jwtSettings);

            return services;
        }

        private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            // Register the Swagger configuration options
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
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
    }

    // Configure Swagger options using IConfigureOptions pattern
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
                    Title = "Generate API",
                    Description = "An ASP.NET Core Web API for managing business entities",
                    Contact = new OpenApiContact
                    {
                        Name = "Generate API Team",
                        Email = "support@generate.com"
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

