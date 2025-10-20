using Infrastructure.Extensions;
using Microsoft.OpenApi.Models;
using Shared.Configurations;
using Shared.Configurations.Database;

namespace Generate.API.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Controllers with JSON options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Generate API",
                    Description = "An ASP.NET Core Web API for managing business entities",
                    Contact = new OpenApiContact
                    {
                        Name = "Generate API Team",
                        Email = "support@generate.com"
                    }
                });

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
}

